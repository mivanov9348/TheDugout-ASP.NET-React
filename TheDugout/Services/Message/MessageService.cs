namespace TheDugout.Services.Message
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Concurrent;
    using System.Text.RegularExpressions;
    using TheDugout.Data;
    using TheDugout.Models.Messages;
    using TheDugout.Services.Message.Interfaces;

    public class MessageService : IMessageService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random = new();

        // ✅ Thread-safe кеш за шаблоните (заменя статичния Dictionary)
        private static readonly ConcurrentDictionary<MessageCategory, List<MessageTemplate>> _templateCache = new();

        // ✅ Regex оставен както е, няма нужда от промяна
        private static readonly Regex _placeholderRegex = new(@"\{([^}|]+)(?:\|([^}]+))?\}", RegexOptions.Compiled);

        public MessageService(DugoutDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Message> CreateMessageAsync(
            MessageCategory category,
            Dictionary<string, string> placeholders,
            int gameSaveId,
            bool strict = false)
        {
            try
            {
                var templates = await GetTemplatesAsync(category);
                if (!templates.Any())
                    throw new InvalidOperationException($"No templates found for category {category}");

                // ✅ Използваме инстанционния Random, не нов всеки път
                var template = PickRandomWeighted(templates);

                var subject = ReplacePlaceholders(template.SubjectTemplate, placeholders, strict);
                var body = ReplacePlaceholders(template.BodyTemplate, placeholders, strict);

                // ✅ Season fallback, за да не гърми, ако няма активен сезон
                var seasonDate = await _context.Seasons
                    .Where(s => s.GameSaveId == gameSaveId && s.IsActive)
                    .Select(s => s.CurrentDate)
                    .FirstOrDefaultAsync();

                var createdAt = seasonDate != default ? seasonDate : DateTime.UtcNow;

                return new Message
                {
                    GameSaveId = gameSaveId,
                    Subject = subject,
                    Body = body,
                    Category = category,
                    SenderType = template.SenderType,
                    CreatedAt = createdAt,
                    IsRead = false,
                    MessageTemplateId = template.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create message for category {Category}", category);
                throw;
            }
        }

        public async Task<Message> CreateAndSaveMessageAsync(
            MessageCategory category,
            Dictionary<string, string> placeholders,
            int gameSaveId,
            bool strict = false)
        {
            var message = await CreateMessageAsync(category, placeholders, gameSaveId, strict);
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        // ✅ Thread-safe кеширане на шаблоните
        private async Task<List<MessageTemplate>> GetTemplatesAsync(MessageCategory category)
        {
            if (_templateCache.TryGetValue(category, out var cached))
                return cached;

            var templates = await _context.MessageTemplates
                .Where(t => t.Category == category)
                .ToListAsync();

            _templateCache[category] = templates;
            return templates;
        }

        // ✅ Позволява ръчно инвалидиране на кеша (ако се добавят нови шаблони)
        public static void ClearCache(MessageCategory? category = null)
        {
            if (category.HasValue)
                _templateCache.TryRemove(category.Value, out _);
            else
                _templateCache.Clear();
        }

        // ✅ Използва общия Random (по-надежден)
        private MessageTemplate PickRandomWeighted(List<MessageTemplate> templates)
        {
            var totalWeight = templates.Sum(t => t.Weight);
            var roll = _random.Next(totalWeight);

            foreach (var t in templates)
            {
                roll -= t.Weight;
                if (roll < 0) return t;
            }

            // fallback (ако нещо стане — но теоретично никога няма да стигне тук)
            return templates.Last();
        }

        // ✅ Safe strict режим и чист fallback
        private string ReplacePlaceholders(string template, Dictionary<string, string> placeholders, bool strict)
        {
            if (string.IsNullOrEmpty(template)) return template;

            return _placeholderRegex.Replace(template, match =>
            {
                var key = match.Groups[1].Value;
                var fallback = match.Groups[2].Success ? match.Groups[2].Value : "";

                if (placeholders.TryGetValue(key, out var value))
                    return value;

                if (strict)
                {
                    _logger.LogWarning("Missing placeholder: {PlaceholderKey}", key);
                }

                // Връщаме fallback или празно, вместо да чупим текста
                return fallback;
            });
        }
    }

}
