using System.Numerics;

namespace TheDugout.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<TeamTemplate> TeamTemplates { get; set; } = new List<TeamTemplate>();
        public ICollection<LeagueTemplate> LeagueTemplates { get; set; } = new List<LeagueTemplate>();
        public ICollection<Player> Players { get; set; } = new List<Player>();

    }
}
