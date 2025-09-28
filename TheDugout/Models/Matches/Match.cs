using TheDugout.Models.Fixtures;
using TheDugout.Models.Game;
using TheDugout.Models.Players;

namespace TheDugout.Models.Matches
{
    public enum MatchStatus
    {
        Scheduled = 0,
        Live = 1,
        Played = 2,
        Cancelled = 3
    }

    public enum MatchTurn
    {
        Home = 1,
        Away = 2
    }

    public class Match
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int FixtureId { get; set; }
        public Fixture Fixture { get; set; } = null!;
        public int CurrentMinute { get; set; } = 0;
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
        public MatchTurn CurrentTurn { get; set; } = MatchTurn.Home;
        public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
        public ICollection<PlayerMatchStats> PlayerStats { get; set; } = new List<PlayerMatchStats>();

    }
}
