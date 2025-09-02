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

    public class FinancialTransaction
    {
        public int Id { get; set; }

        public int? TeamId { get; set; }
        public Team? Team { get; set; }

        public int? BankId { get; set; }
        public Bank? Bank { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public decimal Amount { get; set; } 

        public string Description { get; set; } = string.Empty;

        public TransactionType Type { get; set; }
    }
}
