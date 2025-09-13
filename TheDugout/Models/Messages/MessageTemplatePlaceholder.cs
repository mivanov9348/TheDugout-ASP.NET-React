namespace TheDugout.Models.Messages
{
    public class MessageTemplatePlaceholder
    {
        public int Id { get; set; }

        public int MessageTemplateId { get; set; }
        public MessageTemplate MessageTemplate { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
    }
}
