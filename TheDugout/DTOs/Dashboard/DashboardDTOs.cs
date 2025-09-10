namespace TheDugout.DTOs.Dashboard
{
    public class DashboardDto
    {
        public FinanceDto Finance { get; set; } = new();
        public List<TransferHistoryDto> Transfers { get; set; } = new();

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
}
