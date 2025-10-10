namespace TheDugout.Models.Players
{
    public class PositionWeight
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public int? AttributeId { get; set; }
        public AttributeDefinition Attribute { get; set; } = null!;

        public double Weight { get; set; }
    }
}
