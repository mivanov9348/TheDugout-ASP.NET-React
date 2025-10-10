namespace TheDugout.Models.Staff
{
    using TheDugout.Models.Common;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    public class Agency
    {
        public int Id { get; set; }
        public int AgencyTemplateId { get; set; }
        public AgencyTemplate AgencyTemplate { get; set; } = null!;
        public int RegionId { get; set; }
        public Region Region { get; set; } = null!;
        public int Popularity { get; set; } = 0;    
        public decimal Budget { get; set; }
        public decimal TotalEarnings { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string Logo { get; set; } = null!;
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<FinancialTransaction> TransactionsFrom { get; set; } = new List<FinancialTransaction>();
        public ICollection<FinancialTransaction> TransactionsTo { get; set; } = new List<FinancialTransaction>();
    }
}
