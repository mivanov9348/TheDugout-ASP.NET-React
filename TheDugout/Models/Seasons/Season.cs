namespace TheDugout.Models.Seasons
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Players;
    using TheDugout.Models.Training;
    public class Season
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 7, 1);
        public DateTime EndDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public bool IsActive { get; set; }
        public ICollection<SeasonEvent> Events { get; set; } = new List<SeasonEvent>();
        public ICollection<PlayerSeasonStats> PlayerSeasonStats { get; set; } = new List<PlayerSeasonStats>();
        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();
        public ICollection<EuropeanCup> EuropeanCups { get; set; } = new List<EuropeanCup>();
        public ICollection<Cup> Cups { get; set; } = new List<Cup>();
        public ICollection<Competition> Competitions { get; set; } = new List<Competition>();
        public ICollection<CompetitionSeasonResult> CompetitionSeasonResults { get; set; } = new List<CompetitionSeasonResult>();
        public ICollection<CompetitionAward> Awards { get; set; } = new List<CompetitionAward>();
    }
}
