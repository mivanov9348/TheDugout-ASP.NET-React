using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TheDugout.Data;
using TheDugout.Models.Messages;

namespace TheDugout.Services.Message
{
    public class MessageService : IMessageService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<MessageService> _logger;
        private readonly Random _random = new Random();

        public MessageService(DugoutDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Models.Messages.Message> CreateMessageAsync(
    MessageCategory category,
    Dictionary<string, string> placeholders,
    int? gameSaveId = null,
    bool strict = false)
        {
            var templates = await _context.MessageTemplates
                .Where(t => t.Category == category)
                .ToListAsync();

            if (!templates.Any())
                throw new InvalidOperationException($"No templates found for category {category}");

            var template = PickRandomWeighted(templates);

            var subject = ReplacePlaceholders(template.SubjectTemplate, placeholders, strict);
            var body = ReplacePlaceholders(template.BodyTemplate, placeholders, strict);

            var season = gameSaveId.HasValue
                ? await _context.Seasons
                    .Where(s => s.GameSaveId == gameSaveId.Value && s.IsActive)
                    .FirstOrDefaultAsync()
                : null;

            if (season == null)
                throw new InvalidOperationException("No active season found for this game save");

            return new Models.Messages.Message
            {
                GameSaveId = gameSaveId ?? 0,
                Subject = subject,
                Body = body,
                Category = category,
                SenderType = template.SenderType,
                CreatedAt = season.CurrentDate,
                IsRead = false,
                MessageTemplateId = template.Id,
                MessageTemplate = template
            };
        }


        public async Task<Models.Messages.Message> CreateAndSaveMessageAsync(
            MessageCategory category,
            Dictionary<string, string> placeholders,
            int? gameSaveId = null,
            bool strict = false)
        {
            var message = await CreateMessageAsync(category, placeholders, gameSaveId, strict);

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        private MessageTemplate PickRandomWeighted(List<MessageTemplate> templates)
        {
            var totalWeight = templates.Sum(t => t.Weight);
            var roll = _random.Next(1, totalWeight + 1);
            var cumulative = 0;

            foreach (var t in templates)
            {
                cumulative += t.Weight;
                if (roll <= cumulative)
                    return t;
            }

            return templates.Last(); 
        }

        private string ReplacePlaceholders(string template, Dictionary<string, string> placeholders, bool strict)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var regex = new Regex(@"\{([^}]+)\}");

            return regex.Replace(template, match =>
            {
                var key = match.Groups[1].Value;
                if (placeholders.TryGetValue(key, out var value))
                    return value;

                if (strict)
                    throw new InvalidOperationException($"Missing placeholder: {key}");

                return match.Value; 
            });
        }
    }
}
