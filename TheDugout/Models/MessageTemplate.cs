namespace TheDugout.Models
{
    public class MessageTemplate
    {
        public int Id { get; set; }

        public MessageCategory Category { get; set; }

        public string SubjectTemplate { get; set; } = string.Empty;

        public string BodyTemplate { get; set; } = string.Empty;

        public string? PlaceholdersJson { get; set; }

        public int Weight { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public string Language { get; set; } = "en";

        public ICollection<Message>? Messages { get; set; }
        public ICollection<MessageTemplatePlaceholder>? Placeholders { get; set; }
    }
}
