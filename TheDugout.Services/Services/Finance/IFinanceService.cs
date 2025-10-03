using TheDugout.Models.Finance;
using TheDugout.Models.Game;
using TheDugout.Models.Staff;

namespace TheDugout.Services.Finance
{
    public interface IFinanceService
    {
        Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital = 200_000_000);
        Task InitializeClubFundsAsync(GameSave gameSave, IEnumerable<Models.Leagues.League> leagues);
        Task<FinancialTransaction> ClubToBankAsync(Models.Teams.Team team, Bank bank, decimal amount, string description, TransactionType type);
        Task<FinancialTransaction> BankToClubAsync(Bank bank, Models.Teams.Team team, decimal amount, string description, TransactionType type);
        Task<FinancialTransaction> ClubToClubAsync(Models.Teams.Team fromTeam, Models.Teams.Team toTeam, decimal amount, string description, TransactionType type);
        Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction transaction);
        Task InitializeAgencyFundsAsync(GameSave gameSave, Agency agency);

    }
}
