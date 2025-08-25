namespace TheDugout.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool isRead { get; set; }
        public DateTime Date { get; set; }
    }
}
