using TheDugout.Models.Competitions;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Models.Players;
using TheDugout.Models.Training;

namespace TheDugout.Models.Seasons
{
    public class Season
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; }

        public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 7, 1);
        public DateTime EndDate { get; set; }
        public DateTime CurrentDate { get; set; }

        public ICollection<SeasonEvent> Events { get; set; } = new List<SeasonEvent>();
        public ICollection<PlayerSeasonStats> PlayerStats { get; set; } = new List<PlayerSeasonStats>();
        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();
        public ICollection<EuropeanCup> EuropeanCups { get; set; } = new List<EuropeanCup>();
    }
}
