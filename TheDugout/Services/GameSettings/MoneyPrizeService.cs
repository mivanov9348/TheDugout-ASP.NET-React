namespace TheDugout.Services.GameSettings
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;
    using System;
    using System.Threading.Tasks;

    public class MoneyPrizeService : IMoneyPrizeService
    {
        private readonly DugoutDbContext _context;
        private readonly ITransactionService _transactionService;

        public MoneyPrizeService(DugoutDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<MoneyPrize?> GetByCodeAsync(string code)
        {
            return await _context.MoneyPrizes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
        }
        public async Task<FinancialTransaction?> GrantToTeamAsync(GameSave gameSave, string prizeCode, Team team, string? customDescription = null)
        {
            var prize = await GetByCodeAsync(prizeCode);
            if (prize == null)
                return null;

            var bank = await _context.Banks
        .FirstOrDefaultAsync(b => b.GameSaveId == gameSave.Id);

            if (bank == null)
                throw new InvalidOperationException("Bank not found for the given game save.");

            var description = customDescription ?? prize.Description ?? prize.Name;

            var tx = await _transactionService.BankToClubAsync(bank, team, prize.Amount, description, TransactionType.Prize);

            return tx.Status == TransactionStatus.Completed ? tx : null;
        }
    }
}
