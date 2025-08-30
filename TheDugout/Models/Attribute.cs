namespace TheDugout.Models
{
    public class Attribute
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;   
        public string Name { get; set; } = null!;  

        public ICollection<PlayerAttribute> PlayerAttributes { get; set; } = new List<PlayerAttribute>();
        public ICollection<PositionWeight> PositionWeights { get; set; } = new List<PositionWeight>();
    }
}
