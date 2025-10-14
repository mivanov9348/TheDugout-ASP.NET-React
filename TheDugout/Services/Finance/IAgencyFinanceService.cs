namespace TheDugout.Services.Finance
{
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;

    public interface IAgencyFinanceService
    {
        Task InitializeAgencyFundsAsync(GameSave gameSave, Agency agency);
    }
}
