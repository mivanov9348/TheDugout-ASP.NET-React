namespace TheDugout.Services.Finance
{
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;

    public class TransactionService : ITransactionService
    {
        private readonly DugoutDbContext _context;

        public TransactionService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction tx)
        {
            if (tx == null)
                throw new ArgumentNullException(nameof(tx));

            Bank? bank = null;
            Team? fromTeam = null;
            Team? toTeam = null;
            Agency? fromAgency = null;
            Agency? toAgency = null;

            if (tx.BankId.HasValue)
                bank = await _context.Banks.FindAsync(tx.BankId.Value);
            if (tx.FromTeamId.HasValue)
                fromTeam = await _context.Teams.FindAsync(tx.FromTeamId.Value);
            if (tx.ToTeamId.HasValue)
                toTeam = await _context.Teams.FindAsync(tx.ToTeamId.Value);
            if (tx.FromAgencyId.HasValue)
                fromAgency = await _context.Agencies.FindAsync(tx.FromAgencyId.Value);
            if (tx.ToAgencyId.HasValue)
                toAgency = await _context.Agencies.FindAsync(tx.ToAgencyId.Value);

            // Валидации
            if (fromTeam != null && fromTeam.Balance < tx.Amount)
            {
                tx.Status = TransactionStatus.Failed;
                return tx;
            }

            if (bank != null && tx.ToTeamId.HasValue && bank.Balance < tx.Amount)
            {
                tx.Status = TransactionStatus.Failed;
                return tx;
            }
            if (fromAgency != null && fromAgency.Budget < tx.Amount)
            {
                tx.Status = TransactionStatus.Failed;
                return tx;
            }

            // Прехвърляне
            if (fromTeam != null) fromTeam.Balance -= tx.Amount;
            if (toTeam != null) toTeam.Balance += tx.Amount;
            if (fromAgency != null) fromAgency.Budget -= tx.Amount;
            if (toAgency != null) toAgency.Budget += tx.Amount;

            if (bank != null)
            {
                if (tx.FromTeamId.HasValue || tx.FromAgencyId.HasValue)
                    bank.Balance += tx.Amount;
                else if (tx.ToTeamId.HasValue || tx.ToAgencyId.HasValue)
                    bank.Balance -= tx.Amount;
            }

            tx.Status = TransactionStatus.Completed;
            _context.FinancialTransactions.Add(tx);
            await _context.SaveChangesAsync();

            return tx;
        }
        public Task<FinancialTransaction> BankToClubAsync(Bank b, Team t, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                BankId = b.Id,
                ToTeamId = t.Id,
                GameSaveId = t.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
                Date = GetGameDateOrNow(t.GameSave),
                Status = TransactionStatus.Pending
            });
        public Task<FinancialTransaction> ClubToBankAsync(Team from, Bank bank, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                FromTeamId = from.Id,
                BankId = bank.Id,
                GameSaveId = from.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
                Date = GetGameDateOrNow(from.GameSave),
                Status = TransactionStatus.Pending
            });
        public Task<FinancialTransaction> ClubToAgencyAsync(Team from, Agency to, decimal amt, string desc, TransactionType type)
        => ExecuteTransactionAsync(new FinancialTransaction
        {
            FromTeamId = from.Id,
            ToAgencyId = to.Id,
            GameSaveId = from.GameSaveId,
            Amount = amt,
            Description = desc,
            Type = type,
            Date = GetGameDateOrNow(from.GameSave),
            Status = TransactionStatus.Pending
        });
        public Task<FinancialTransaction> BankToAgencyAsync(Bank from, Agency to, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                BankId = from.Id,
                ToAgencyId = to.Id,
                GameSaveId = from.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
                Date = GetGameDateOrNow(from.GameSave),
                Status = TransactionStatus.Pending
            });
        public Task<FinancialTransaction> AgencyToClubAsync(Agency from, Team to, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                FromAgencyId = from.Id,
                ToTeamId = to.Id,
                GameSaveId = from.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
                Date = GetGameDateOrNow(from.GameSave),
                Status = TransactionStatus.Pending
            });

        public Task<FinancialTransaction> AgencyToBankAsync(Agency from, Bank bank, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                FromAgencyId = from.Id,
                BankId = bank.Id,
                GameSaveId = from.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
                Date = GetGameDateOrNow(from.GameSave),
                Status = TransactionStatus.Pending
            });

        private static DateTime GetGameDateOrNow(GameSave? gameSave)
        {
            return gameSave?.CurrentSeason?.CurrentDate
                   ?? gameSave?.CurrentSeason?.StartDate
                   ?? DateTime.Now;
        }

    }
}
