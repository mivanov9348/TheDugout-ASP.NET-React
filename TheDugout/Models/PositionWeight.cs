namespace TheDugout.Models
{
    public class PositionWeight
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public string AttributeName { get; set; } = null!;
        public double Weight { get; set; } // примерно 0.0 – 1.0
    }
}
