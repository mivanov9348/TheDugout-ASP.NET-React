using TheDugout.Models.Common;
using TheDugout.Models.Enums;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Matches;

namespace TheDugout.Models.Players
{
    public class PlayerMatchStats
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public int MatchId { get; set; }
        public Match Match { get; set; }
        public int CompetitionId { get; set; }
        public Competition Competition { get; set; } = null!;
        public int Goals { get; set; }
    }
}
