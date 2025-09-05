namespace TheDugout.Models
{

    public enum MessageCategory
    {
        Welcome,
        Transfer,
        MatchResult,
        General

    }
    public class Message
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public int? GameSaveId { get; set; }
        public GameSave? GameSave { get; set; } = null!;

        public int? MessageTemplateId { get; set; }
        public MessageTemplate? MessageTemplate { get; set; }
    }

}
