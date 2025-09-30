using System.Numerics;
using TheDugout.Models.Leagues;
using TheDugout.Models.Players;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Common
{
    public class Country
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   
        public string Name { get; set; } = null!;

        public string RegionCode { get; set; } = null!;
        public Region Region { get; set; } = null!;

        public ICollection<TeamTemplate> TeamTemplates { get; set; } = new List<TeamTemplate>();
        public ICollection<LeagueTemplate> LeagueTemplates { get; set; } = new List<LeagueTemplate>();
        public ICollection<Player> Players { get; set; } = new List<Player>();

    }
}
