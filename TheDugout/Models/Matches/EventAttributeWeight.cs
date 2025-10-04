using System.ComponentModel.DataAnnotations;
using TheDugout.Models.Players;

namespace TheDugout.Models.Matches
{
    public class EventAttributeWeight
    {
        public int Id { get; set; }
        public string EventTypeCode { get; set; } = string.Empty;
        public EventType EventType { get; set; } = null!;
        [MaxLength(50)]

        public string AttributeCode { get; set; } = string.Empty;
        public AttributeDefinition Attribute { get; set; } = null!;
        public double Weight { get; set; }
    }
}
