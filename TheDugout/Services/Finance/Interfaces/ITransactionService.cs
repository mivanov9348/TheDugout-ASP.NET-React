namespace TheDugout.Services.Finance.Interfaces
{
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;

    public interface ITransactionService
    {
        Task<FinancialTransaction> ExecuteTransactionAsync(FinancialTransaction tx);
        Task<FinancialTransaction> BankToClubAsync(Bank b, Team t, decimal amt, string desc, TransactionType type);
        Task<FinancialTransaction> ClubToBankAsync(Team from, Bank bank, decimal amt, string desc, TransactionType type);
        Task<FinancialTransaction> ClubToAgencyAsync(Team from, Agency to, decimal amt, string desc, TransactionType type);
        Task<FinancialTransaction> BankToAgencyAsync(Bank from, Agency to, decimal amt, string desc, TransactionType type);
        Task<FinancialTransaction> AgencyToClubAsync(Agency from, Team to, decimal amt, string desc, TransactionType type);
        Task<FinancialTransaction> AgencyToBankAsync(Agency from, Bank bank, decimal amt, string desc, TransactionType type);


    }
}
