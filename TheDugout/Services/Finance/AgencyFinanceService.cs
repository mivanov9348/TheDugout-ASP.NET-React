namespace TheDugout.Services.Finance
{
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    using TheDugout.Services.Finance.Interfaces;

    public class AgencyFinanceService : IAgencyFinanceService
    {
        private readonly DugoutDbContext _context;
        private readonly Random _rng = new();

        public AgencyFinanceService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAgencyFundsAsync(GameSave gameSave, Agency agency)
        {
            var bank = gameSave.Bank ?? throw new InvalidOperationException("GameSave.Bank is null");

            var baseBudget = 1_000_000m;
            var popularityAdjustment = (agency.Popularity - 2) * 200_000m;
            var randomVariance = _rng.Next(-100_000, 100_000);
            var initialFunds = Math.Clamp(baseBudget + popularityAdjustment + randomVariance, 600_000m, 1_400_000m);

            agency.Budget = initialFunds;
            bank.Balance -= initialFunds;

            var tx = new FinancialTransaction
            {
                BankId = bank.Id,
                ToAgencyId = agency.Id,
                GameSaveId = gameSave.Id,
                Amount = initialFunds,
                Description = $"Starting funds for {agency.AgencyTemplate.Name}",
                Type = TransactionType.StartingFunds,
                Status = TransactionStatus.Completed
            };

            _context.FinancialTransactions.Add(tx);
            await _context.SaveChangesAsync();
        }
    }
}
