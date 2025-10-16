namespace TheDugout.Services.Message
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.RegularExpressions;
    using TheDugout.Data;
    using TheDugout.Models.Messages;
    using TheDugout.Services.Message.Interfaces;

    public class MessageService : IMessageService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random = new();
        private static readonly Dictionary<MessageCategory, List<MessageTemplate>> _templateCache = new();
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

                var template = PickRandomWeighted(templates);
                var subject = ReplacePlaceholders(template.SubjectTemplate, placeholders, strict);
                var body = ReplacePlaceholders(template.BodyTemplate, placeholders, strict);

                var season = await _context.Seasons
                    .Where(s => s.GameSaveId == gameSaveId && s.IsActive)
                    .Select(s => s.CurrentDate)
                    .FirstOrDefaultAsync();

                if (season == default)
                    throw new InvalidOperationException("No active season found for this game save");

                return new Message
                {
                    GameSaveId = gameSaveId,
                    Subject = subject,
                    Body = body,
                    Category = category,
                    SenderType = template.SenderType,
                    CreatedAt = season,
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
        private static MessageTemplate PickRandomWeighted(List<MessageTemplate> templates)
        {
            var roll = new Random().Next(templates.Sum(t => t.Weight));
            return templates.First(t => (roll -= t.Weight) < 0);
        }
        private static string ReplacePlaceholders(string template, Dictionary<string, string> placeholders, bool strict)
        {
            if (string.IsNullOrEmpty(template)) return template;

            return _placeholderRegex.Replace(template, match =>
            {
                var key = match.Groups[1].Value;
                var fallback = match.Groups[2].Success ? match.Groups[2].Value : match.Value;
                if (placeholders.TryGetValue(key, out var value))
                    return value;
                if (strict)
                    throw new InvalidOperationException($"Missing placeholder: {key}");
                return fallback;
            });
        }
    }
}
