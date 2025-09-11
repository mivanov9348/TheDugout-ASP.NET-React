namespace TheDugout.Models
{
    public enum MatchStatus { Scheduled = 0, Played = 1, Cancelled = 2 }

    public class EuropeanCupMatch
    {
        public int Id { get; set; }

        public int PhaseId { get; set; }
        public EuropeanCupPhase Phase { get; set; } = null!;

        public int HomeCupTeamId { get; set; }
        public EuropeanCupTeam HomeCupTeam { get; set; } = null!;

        public int AwayCupTeamId { get; set; }
        public EuropeanCupTeam AwayCupTeam { get; set; } = null!;

        public int Leg { get; set; } = 0;
        public DateTime? MatchDate { get; set; }

        public int? HomeGoals { get; set; }
        public int? AwayGoals { get; set; }

        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    }
}
