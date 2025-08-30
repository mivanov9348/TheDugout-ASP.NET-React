namespace TheDugout.Models
{
    public class PositionWeight
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public int AttributeId { get; set; }
        public Attribute Attribute { get; set; } = null!;

        public double Weight { get; set; }
    }
}
