using TheDugout.Models;

namespace TheDugout.Services.Finance
{
    public interface IFinanceService
    {
        Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital = 200_000_000);
        Task InitializeClubFundsAsync(GameSave gameSave, IEnumerable<Models.League> leagues);


        Task<FinancialTransaction> ClubToBankAsync(Models.Team team, Bank bank, decimal amount, string description, TransactionType type);
        Task<FinancialTransaction> BankToClubAsync(Bank bank, Models.Team team, decimal amount, string description, TransactionType type);
        Task<FinancialTransaction> ClubToClubAsync(Models.Team fromTeam, Models.Team toTeam, decimal amount, string description, TransactionType type);
                Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction transaction);
    }
}
