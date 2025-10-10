using TheDugout.Models.Players;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Matches
{
    public class Penalty
    {
        public int Id { get; set; }

        public int? MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public int? TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int? PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int Order { get; set; }
        public bool IsScored { get; set; }
    }

}
