namespace TheDugout.Models
{
    public enum TransactionType
    {
        TransferIn,
        TransferOut,
        TransferFee,
        Wage,
        Prize,
        LoanPayment,
        LoanReceived,
        Misc
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed
    }

    public class FinancialTransaction
    {
        public int Id { get; set; }

        public int? FromTeamId { get; set; }
        public Team? FromTeam { get; set; }

        public int? ToTeamId { get; set; }
        public Team? ToTeam { get; set; }

        public int? BankId { get; set; }
        public Bank? Bank { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public decimal Amount { get; set; }

        public decimal Fee { get; set; } = 0m;

        public string Description { get; set; } = string.Empty;

        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    }
}
