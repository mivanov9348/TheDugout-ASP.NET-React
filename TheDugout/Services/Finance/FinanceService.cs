using TheDugout.Data;
using TheDugout.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<FinancialTransaction> ClubToBankAsync(Models.Team team, Bank bank, decimal amount, string description, TransactionType type)
        {
            var tx = new FinancialTransaction
            {
                FromTeamId = team.Id,
                BankId = bank.Id,
                Amount = amount,
                Description = description,
                Type = type,
                Status = TransactionStatus.Pending
            };

            return await ExecuteTransactionAsync(tx);
        }

        public async Task<FinancialTransaction> BankToClubAsync(Bank bank, Models.Team team, decimal amount, string description, TransactionType type)
        {
            var tx = new FinancialTransaction
            {
                ToTeamId = team.Id,
                BankId = bank.Id,
                Amount = amount,
                Description = description,
                Type = type,
                Status = TransactionStatus.Pending
            };

            return await ExecuteTransactionAsync(tx);
        }

        public async Task<FinancialTransaction> ClubToClubAsync(Models.Team fromTeam, Models.Team toTeam, decimal amount, string description, TransactionType type)
        {
            var tx = new FinancialTransaction
            {
                FromTeamId = fromTeam.Id,
                ToTeamId = toTeam.Id,
                Amount = amount,
                Description = description,
                Type = type,
                Status = TransactionStatus.Pending
            };

            return await ExecuteTransactionAsync(tx);
        }

        public async Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction transaction)
        {
            // Валидация за достатъчен баланс от изпращащия
            if (transaction.FromTeamId.HasValue)
            {
                var fromTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Id == transaction.FromTeamId.Value);

                if (fromTeam == null)
                {
                    transaction.Status = TransactionStatus.Failed;
                    _logger.LogError("Transaction failed: FromTeam {TeamId} not found", transaction.FromTeamId.Value);
                    return transaction;
                }

                if (fromTeam.Balance < transaction.Amount)
                {
                    transaction.Status = TransactionStatus.Failed;
                    _logger.LogWarning("Transaction failed: Team {TeamId} has insufficient funds", fromTeam.Id);
                    return transaction;
                }

                fromTeam.Balance -= transaction.Amount;
            }

            if (transaction.ToTeamId.HasValue)
            {
                var toTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Id == transaction.ToTeamId.Value);

                if (toTeam == null)
                {
                    transaction.Status = TransactionStatus.Failed;
                    _logger.LogError("Transaction failed: ToTeam {TeamId} not found", transaction.ToTeamId.Value);
                    return transaction;
                }

                toTeam.Balance += transaction.Amount;
            }

            if (transaction.BankId.HasValue)
            {
                var bank = await _context.Banks
                    .FirstOrDefaultAsync(b => b.Id == transaction.BankId.Value);

                if (bank == null)
                {
                    transaction.Status = TransactionStatus.Failed;
                    _logger.LogError("Transaction failed: Bank {BankId} not found", transaction.BankId.Value);
                    return transaction;
                }

                if (transaction.FromTeamId.HasValue)
                {
                    bank.Balance += transaction.Amount;
                }
                else if (transaction.ToTeamId.HasValue)
                {
                    if (bank.Balance < transaction.Amount)
                    {
                        transaction.Status = TransactionStatus.Failed;
                        _logger.LogWarning("Transaction failed: Bank {BankId} has insufficient funds", bank.Id);
                        return transaction;
                    }

                    bank.Balance -= transaction.Amount;
                }
            }

            transaction.Status = TransactionStatus.Completed;
            _context.FinancialTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction executed: {Description}, Amount {Amount}, Type {Type}",
                transaction.Description, transaction.Amount, transaction.Type);

            return transaction;
        }

    }
}
