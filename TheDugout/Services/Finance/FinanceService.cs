using TheDugout.Data;
using TheDugout.Models;
using Microsoft.Extensions.Logging;

namespace TheDugout.Services.Finance
{
    public class FinanceService : IFinanceService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<FinanceService> _logger;

        public FinanceService(DugoutDbContext context, ILogger<FinanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital)
        {
            var bank = new Bank
            {
                Balance = initialCapital,
                Transactions = new List<FinancialTransaction>()
            };

            gameSave.Bank = bank;

            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bank created for GameSave {GameSaveId} with initial capital {Capital}",
                gameSave.Id, initialCapital);

            return bank;
        }
    }
}
