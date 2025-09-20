using System.Text.Json.Serialization;

namespace TheDugout.Models.Players
{
    public enum AttributeCategory
    {
        Physical = 1,
        Technical = 2,
        Mental = 3,
        Goalkeeping = 4
    }

    public class Attribute
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public AttributeCategory Category { get; set; }

        public ICollection<PlayerAttribute> PlayerAttributes { get; set; } = new List<PlayerAttribute>();
        public ICollection<PositionWeight> PositionWeights { get; set; } = new List<PositionWeight>();
    }

}
