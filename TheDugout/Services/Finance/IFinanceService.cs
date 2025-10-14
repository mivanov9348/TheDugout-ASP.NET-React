namespace TheDugout.Services.Finance
{
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    public interface IFinanceService
    {
        Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital);
        Task InitializeClubFundsAsync(GameSave gameSave, IEnumerable<Models.Leagues.League> leagues);
        Task<FinancialTransaction> BankToClubAsync(Bank bank, Models.Teams.Team team, decimal amount, string description, TransactionType type);
        Task<(bool Success, string ErrorMessage)> ClubToClubWithFeeAsync(
                            Models.Teams.Team buyer,
                            Models.Teams.Team seller,
                            Bank bank,
                            decimal transferAmount,
                            string description);
        Task<FinancialTransaction> ClubToBankAsync(Models.Teams.Team from, Bank bank, decimal amt, string desc, TransactionType type);
        Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction transaction);
        Task InitializeAgencyFundsAsync(GameSave gameSave, Agency agency);

    }
}
