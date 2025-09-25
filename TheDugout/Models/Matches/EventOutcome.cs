namespace TheDugout.Models.Matches
{
    public class EventOutcome
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EventTypeCode { get; set; } = string.Empty;
        public bool ChangesPossession { get; set; }
        public int Weight { get; set; }
        public int EventTypeId { get; set; }
        public EventType EventType { get; set; } = null!;
        public ICollection<CommentaryTemplate> CommentaryTemplates { get; set; } = new List<CommentaryTemplate>();
        public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();

    }
}
