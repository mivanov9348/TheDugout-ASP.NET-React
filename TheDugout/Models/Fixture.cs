namespace TheDugout.Models
{
    public enum CompetitionType
    {
        League = 0,
        DomesticCup = 1,
        EuropeanCup = 2
    }

    public enum MatchStatus
    {
        Scheduled = 0,
        Played = 1,
        Cancelled = 2
    }

    public class Fixture
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public CompetitionType CompetitionType { get; set; }

        public int? LeagueId { get; set; }
        public League? League { get; set; }

        //public int? CupId { get; set; }
        //public Cup? Cup { get; set; }

        public int? EuropeanCupPhaseId { get; set; }
        public EuropeanCupPhase? EuropeanCupPhase { get; set; }

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        public int? HomeTeamGoals { get; set; }
        public int? AwayTeamGoals { get; set; }

        public DateTime Date { get; set; }

        public int Round { get; set; }

        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    }
}
