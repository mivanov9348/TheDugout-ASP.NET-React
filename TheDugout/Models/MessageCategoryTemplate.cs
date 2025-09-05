namespace TheDugout.Models
{
    public class MessageCategoryTemplate
    {
        public List<string> Placeholders { get; set; } = new List<string>();
        public List<MessageTemplate> Templates { get; set; } = new List<MessageTemplate>();
    }
}
