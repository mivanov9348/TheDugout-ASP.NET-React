using TheDugout.Models.Game;

namespace TheDugout.Models.Matches
{
    public enum MatchStatus
    {
        Scheduled = 0,
        Live = 1,
        Played = 2,
        Cancelled = 3
    }
    public class Match
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int FixtureId { get; set; }
        public Fixture Fixture { get; set; } = null!;

        public int CurrentMinute { get; set; } = 0;
        public MatchStatus Status { get; set; } = MatchStatus.Live;

        public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
    }


}
