namespace TheDugout.Models.Matches
{
    public class EventType
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ICollection<EventOutcome> Outcomes { get; set; } = new List<EventOutcome>();
        public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
        public ICollection<EventAttributeWeight> AttributeWeights { get; set; } = new List<EventAttributeWeight>();
    }
}
