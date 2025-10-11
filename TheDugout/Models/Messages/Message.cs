using TheDugout.Models.Game;

namespace TheDugout.Models.Messages
{
    public enum MessageCategory
    {
        Welcome,
        Transfer,
        MatchResult,
        Board,
        Fans,
        Media,
        Injury,
        Training,
        YouthAcademy,
        Finance,
        Milestone,
        Scouting,
        Competition,
        General
    }
    public enum MessageSenderType
    {
        System,
        Board,
        Assistant,
        Player,
        Media,
        Fans
    }
    public class Message
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public MessageCategory Category { get; set; }
        public MessageSenderType SenderType { get; set; } = MessageSenderType.System;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int? MessageTemplateId { get; set; }
        public MessageTemplate? MessageTemplate { get; set; }
    }

}
