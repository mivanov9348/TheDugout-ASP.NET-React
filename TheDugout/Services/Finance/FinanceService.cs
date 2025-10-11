namespace TheDugout.Services.Finance
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;
    public class FinanceService : IFinanceService
    {
        private readonly DugoutDbContext _context;
        private readonly Random _rng = new();

        public FinanceService(DugoutDbContext context)
        {
            _context = context;
        }

        // 🔹 1️⃣ Създаване на банка
        public async Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital)
        {
            var bank = new Bank
            {
                Balance = initialCapital,
                GameSaveId = gameSave.Id
            };

            gameSave.Bank = bank;
            _context.Banks.Add(bank);

            await _context.SaveChangesAsync();     

            return bank;
        }

        // 🔹 2️⃣ Масова инициализация на клубни фондове
        public async Task InitializeClubFundsAsync(GameSave gameSave, IEnumerable<Models.Leagues.League> leagues)
        {
            var bank = gameSave.Bank ?? throw new InvalidOperationException("GameSave.Bank is null");
            var transactions = new List<FinancialTransaction>();
            decimal totalAllocated = 0;

            foreach (var league in leagues)
            {
                foreach (var team in league.Teams)
                {
                    var initialFunds = team.Popularity * 1_000m + 50_000m;

                    team.Balance += initialFunds;
                    bank.Balance -= initialFunds;
                    totalAllocated += initialFunds;

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

        // 🔹 3️⃣ Инициализация на агенции
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

        // 🔹 4️⃣ Унифициран метод за runtime трансакции
        public async Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction tx)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));

            // Валидираме само участниците, които имат нужда
            Bank? bank = null;
            Team? fromTeam = null;
            Team? toTeam = null;

            if (tx.BankId.HasValue)
                bank = await _context.Banks.FindAsync(tx.BankId.Value);

            if (tx.FromTeamId.HasValue)
                fromTeam = await _context.Teams.FindAsync(tx.FromTeamId.Value);

            if (tx.ToTeamId.HasValue)
                toTeam = await _context.Teams.FindAsync(tx.ToTeamId.Value);

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

            // Прехвърляне
            if (fromTeam != null) fromTeam.Balance -= tx.Amount;
            if (toTeam != null) toTeam.Balance += tx.Amount;

            if (bank != null)
            {
                if (tx.FromTeamId.HasValue) bank.Balance += tx.Amount;
                else if (tx.ToTeamId.HasValue) bank.Balance -= tx.Amount;
            }

            tx.Status = TransactionStatus.Completed;
            _context.FinancialTransactions.Add(tx);
            await _context.SaveChangesAsync();

            return tx;
        }

        // 🔹 5️⃣ Шорткъти (по избор)
        public Task<FinancialTransaction> BankToClubAsync(Bank b, Team t, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                BankId = b.Id,
                ToTeamId = t.Id,
                GameSaveId = t.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
                Status = TransactionStatus.Pending
            });

        public Task<FinancialTransaction> ClubToClubAsync(Team from, Team to, decimal amt, string desc, TransactionType type)
            => ExecuteTransactionAsync(new FinancialTransaction
            {
                FromTeamId = from.Id,
                ToTeamId = to.Id,
                GameSaveId = from.GameSaveId,
                Amount = amt,
                Description = desc,
                Type = type,
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
        Status = TransactionStatus.Pending
    });

    }
}
