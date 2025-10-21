namespace TheDugout.Services.Finance.Interfaces
{
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Teams;

    public interface ITeamFinanceService
    {
        Task InitializeClubFundsAsync(GameSave gameSave, IEnumerable<League> leagues);
        Task<(bool Success, string ErrorMessage)> ClubToClubWithFeeAsync(
            Team buyer, Team seller, Bank bank, decimal transferAmount, string description);
    }
}
