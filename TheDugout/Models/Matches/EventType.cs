namespace TheDugout.Models.Matches
{
    public class EventType
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty; 
        public int BaseSuccessRate { get; set; }
        public ICollection<EventOutcome> Outcomes { get; set; } = new List<EventOutcome>();
        public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();

    }
}
