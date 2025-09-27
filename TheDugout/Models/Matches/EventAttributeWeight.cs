using TheDugout.Models.Players;

namespace TheDugout.Models.Matches
{
    public class EventAttributeWeight
    {
        public int Id { get; set; }

        public string EventTypeCode { get; set; } = string.Empty;
        public EventType EventType { get; set; } = null!;

        public string AttributeCode { get; set; } = string.Empty;
        public PlayerAttribute Attribute { get; set; } = null!;
        public double Weight { get; set; }
    }

}
