namespace TheDugout.Models.Common
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;
    public class Competition
    {
        public int Id { get; set; }
        public CompetitionTypeEnum Type { get; set; }
        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public League? League { get; set; } = null!;
        public int? LeagueId { get; set; }
        public Cup? Cup { get; set; } = null!;
        public int? CupId { get; set; }
        public EuropeanCup? EuropeanCup { get; set; } = null!;
        public int? EuropeanCupId { get; set; }
        public int? GameSaveId { get; set; }
        public GameSave GameSave { get; set; }
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<PlayerMatchStats> PlayerStats { get; set; } = new List<PlayerMatchStats>();
    }
}
