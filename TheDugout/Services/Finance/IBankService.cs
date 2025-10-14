namespace TheDugout.Services.Finance
{
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;

    public interface IBankService
    {
        Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital);
        Task DepositAsync(Bank bank, decimal amount);
        Task WithdrawAsync(Bank bank, decimal amount);
    }
}
