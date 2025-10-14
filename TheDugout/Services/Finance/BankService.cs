namespace TheDugout.Services.Finance
{
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;

    public class BankService : IBankService
    {
        private readonly DugoutDbContext _context;

        public BankService(DugoutDbContext context)
        {
            _context = context;
        }

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

        public async Task DepositAsync(Bank bank, decimal amount)
        {
            bank.Balance += amount;
            await _context.SaveChangesAsync();
        }

        public async Task WithdrawAsync(Bank bank, decimal amount)
        {
            if (bank.Balance < amount)
                throw new InvalidOperationException("Insufficient bank funds.");

            bank.Balance -= amount;
            await _context.SaveChangesAsync();
        }
    }
}
