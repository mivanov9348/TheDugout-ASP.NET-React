namespace TheDugout.Services.Finance
{
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;

    public class AgencyFinanceService : IAgencyFinanceService
    {
        private readonly DugoutDbContext _context;
        private readonly IGameSettingsService _gameSettingsService;
        private readonly Random _rng = new();

        public AgencyFinanceService(DugoutDbContext context, IGameSettingsService gameSettingsService)
        {
            _context = context;
            _gameSettingsService = gameSettingsService;

        }

        public async Task InitializeAgencyFundsAsync(GameSave gameSave, Agency agency)
        {
            var bank = gameSave.Bank ?? throw new InvalidOperationException("GameSave.Bank is null");

            // >>> ВЗИМАМЕ стойности от GameSettingsService
            var baseBudget = await _gameSettingsService.GetDecimalAsync("InitialAgencyBudget") ?? 1_000_000m;
            var popularityAdjustmentStep = await _gameSettingsService.GetDecimalAsync("AgencyPopularityAdjustmentStep") ?? 200_000m;
            var minFunds = await _gameSettingsService.GetDecimalAsync("AgencyInitialFundsMin") ?? 600_000m;
            var maxFunds = await _gameSettingsService.GetDecimalAsync("AgencyInitialFundsMax") ?? 1_400_000m;

            // >>> Калкулираме на база на популярността и малка случайност
            var popularityAdjustment = (agency.Popularity - 2) * popularityAdjustmentStep;
            var randomVariance = _rng.Next(-100_000, 100_000);
            var initialFunds = Math.Clamp(baseBudget + popularityAdjustment + randomVariance, minFunds, maxFunds);

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
