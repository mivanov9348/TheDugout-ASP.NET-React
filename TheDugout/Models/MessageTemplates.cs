namespace TheDugout.Models
{
    public class MessageTemplates
    {
        public Dictionary<string, MessageCategoryTemplate> Categories { get; set; } = new Dictionary<string, MessageCategoryTemplate>();
    }
}
