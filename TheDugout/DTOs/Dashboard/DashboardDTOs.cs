namespace TheDugout.DTOs.Dashboard
{
    public class DashboardDto
    {
        public FinanceDto Finance { get; set; } = new();
        public List<TransferHistoryDto> Transfers { get; set; } = new();
        public NextMatchDto? NextMatch { get; set; }
        public List<LastFixtureDto> LastFixtures { get; set; } = new();
        public StandingDto? Standing { get; set; }   

    }

    public class LastFixtureDto
    {
        public DateTime Date { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string Competition { get; set; } = string.Empty;
        public int? HomeGoals { get; set; }
        public int? AwayGoals { get; set; }
    }

    public class StandingDto
    {
        public string League { get; set; } = string.Empty;
        public int Ranking { get; set; }
        public int Matches { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
        public int Points { get; set; }
    }

    public class FinanceDto
    {
        public decimal CurrentBalance { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; } = new();
    }

    public class TransactionDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class TransferHistoryDto
    {
        public int Id { get; set; }
        public string Player { get; set; } = string.Empty;
        public string FromTeam { get; set; } = string.Empty;
        public string ToTeam { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public DateTime GameDate { get; set; }
        public bool IsFreeAgent { get; set; }
        public string Season { get; set; } = string.Empty;
    }

    public class NextMatchDto   
    {
        public DateTime Date { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string Competition { get; set; } = string.Empty;
    }
}
