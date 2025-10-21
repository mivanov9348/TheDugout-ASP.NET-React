namespace TheDugout.Services.Finance
{
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;

    public class TeamFinanceService : ITeamFinanceService
    {
        private readonly DugoutDbContext _context;
        private readonly ITransactionService _transactions;
        private readonly IGameSettingsService _settings;

        public TeamFinanceService(DugoutDbContext context, ITransactionService transactions, IGameSettingsService settings)
        {
            _context = context;
            _transactions = transactions;
            _settings = settings;
        }

        public async Task InitializeClubFundsAsync(GameSave gameSave, IEnumerable<League> leagues)
        {
            var bank = gameSave.Bank ?? throw new InvalidOperationException("GameSave.Bank is null");
            var transactions = new List<FinancialTransaction>();

            foreach (var league in leagues)
            {
                foreach (var team in league.Teams)
                {
                    var initialFunds = team.Popularity * 1_000m + 50_000m;

                    team.Balance += initialFunds;
                    bank.Balance -= initialFunds;

                    transactions.Add(new FinancialTransaction
                    {
                        BankId = bank.Id,
                        ToTeamId = team.Id,
                        GameSaveId = gameSave.Id,
                        Amount = initialFunds,
                        Description = $"Starting funds: {initialFunds:N0} to {team.Name}",
                        Type = TransactionType.StartingFunds,
                        Status = TransactionStatus.Completed
                    });
                }
            }

            _context.FinancialTransactions.AddRange(transactions);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, string ErrorMessage)> ClubToClubWithFeeAsync(
            Team buyer, Team seller, Bank bank, decimal transferAmount, string description)
        {
            if (buyer == null || seller == null || bank == null)
                return (false, "Invalid transfer participants.");

            if (transferAmount <= 0)
                return (false, "Invalid transfer amount.");

            var feePercent = await _settings.GetDecimalAsync("bankFeePercent") ?? 0.10m;
            decimal bankFee = Math.Round(transferAmount * feePercent, 2);
            decimal sellerAmount = transferAmount - bankFee;

            var toBank = await _transactions.ClubToBankAsync(
                buyer, bank, transferAmount,
                $"{description} (transfer payment to bank)", TransactionType.TransferFee);

            if (toBank.Status != TransactionStatus.Completed)
                return (false, "Buyer payment to bank failed.");

            var toSeller = await _transactions.BankToClubAsync(
                bank, seller, sellerAmount,
                $"{description} (seller receives 90% after bank fee)", TransactionType.TransferIn);

            if (toSeller.Status != TransactionStatus.Completed)
                return (false, "Bank failed to send funds to seller.");

            var feeTx = new FinancialTransaction
            {
                BankId = bank.Id,
                GameSaveId = buyer.GameSaveId,
                Amount = bankFee,
                Description = $"Bank fee ({feePercent:P0}) from transfer: {description}",
                Type = TransactionType.BankFee,
                Status = TransactionStatus.Completed
            };

            _context.FinancialTransactions.Add(feeTx);
            await _context.SaveChangesAsync();

            return (true, "");
        }
    }
}
