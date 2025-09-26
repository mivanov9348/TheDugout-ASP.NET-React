using TheDugout.Models.Competitions;
using TheDugout.Models.Finance;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Matches;
using TheDugout.Models.Messages;
using TheDugout.Models.Players;
using TheDugout.Models.Seasons;
using TheDugout.Models.Staff;
using TheDugout.Models.Teams;
using TheDugout.Models.Training;

namespace TheDugout.Models.Game
{
    public class GameSave
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = "New Save";

        public int? UserTeamId { get; set; }
        public Team? UserTeam { get; set; }

        public int BankId { get; set; }
        public Bank Bank { get; set; } = null!;

        public string NextDayActionLabel { get; set; } = "Next Day →";

        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<Season> Seasons { get; set; } = new List<Season>();
        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
        public ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();
        public ICollection<Cup> Cups { get; set; } = new List<Cup>();
        public ICollection<Agency> Agencies { get; set; } = new List<Agency>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();


    }
}
