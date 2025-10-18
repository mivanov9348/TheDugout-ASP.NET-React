namespace TheDugout.Models.Matches
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;  
   
    public class Match
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int FixtureId { get; set; }
        public Fixture Fixture { get; set; } = null!;
        public int CurrentMinute { get; set; } = 0;
        public Competition? Competition { get; set; } = null!;
        public int CompetitionId { get; set; }
        public MatchStageEnum Status { get; set; } = MatchStageEnum.Scheduled;
        public MatchTurn CurrentTurn { get; set; } = MatchTurn.Home;
        public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
        public ICollection<PlayerMatchStats> PlayerStats { get; set; } = new List<PlayerMatchStats>();
        public ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
    }
}
