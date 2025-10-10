namespace TheDugout.Models.Players
{
    public class PlayerAttribute
    {
        public int Id { get; set; }

        public int? PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int? AttributeId { get; set; }
        public AttributeDefinition Attribute { get; set; } = null!;

        public int Value { get; set; }
        public double Progress { get; set; } = 0.0;
    }
}
