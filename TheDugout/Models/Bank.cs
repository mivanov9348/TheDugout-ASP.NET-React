namespace TheDugout.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public decimal Balance { get; set; }
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }
}
