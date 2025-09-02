namespace TheDugout.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }

        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }
}
