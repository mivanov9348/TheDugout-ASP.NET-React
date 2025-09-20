namespace TheDugout.Models.Messages
{
    public class MessageTemplate
    {
        public int Id { get; set; }
        public MessageCategory Category { get; set; }
        public string SubjectTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public MessageSenderType SenderType { get; set; } = MessageSenderType.System;
        public int Weight { get; set; } = 1;
    }

}
