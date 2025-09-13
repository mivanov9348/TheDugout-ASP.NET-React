namespace TheDugout.Models.Players
{
    public class Position
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   
        public string Name { get; set; } = null!;  

        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<PositionWeight> Weights { get; set; } = new List<PositionWeight>();
    }
}
