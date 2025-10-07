using TheDugout.Models.Competitions;
using TheDugout.Models.Cups;
using TheDugout.Models.Enums;
using TheDugout.Models.Game;
using TheDugout.Models.Leagues;
using TheDugout.Models.Matches;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Fixtures
{      

    public class Fixture
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public CompetitionTypeEnum CompetitionType { get; set; }

        public int? LeagueId { get; set; }
        public League? League { get; set; }

        public int? CupRoundId { get; set; }
        public CupRound? CupRound { get; set; }

        public int? EuropeanCupPhaseId { get; set; }
        public EuropeanCupPhase? EuropeanCupPhase { get; set; }

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        public int? HomeTeamGoals { get; set; }
        public int? AwayTeamGoals { get; set; }

        public int? WinnerTeamId { get; set; }
        public Team? WinnerTeam { get; set; }

        public DateTime Date { get; set; }

        public int Round { get; set; }
        public bool IsElimination { get; set; } = false;

        public FixtureStatusEnum Status { get; set; } = FixtureStatusEnum.Scheduled;
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
