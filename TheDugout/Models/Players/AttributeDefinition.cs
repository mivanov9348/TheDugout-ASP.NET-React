namespace TheDugout.Models.Players
{
    using System.ComponentModel.DataAnnotations;
    public enum AttributeCategory
    {
        Physical = 1,
        Technical = 2,
        Mental = 3,
        Goalkeeping = 4
    }
    public class AttributeDefinition
    {
        public int Id { get; set; }
        [MaxLength(50)]

        public string Code { get; set; } = null!;
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public AttributeCategory Category { get; set; }
        public ICollection<PlayerAttribute> PlayerAttributes { get; set; } = new List<PlayerAttribute>();
        public ICollection<PositionWeight> PositionWeights { get; set; } = new List<PositionWeight>();
    }
}
