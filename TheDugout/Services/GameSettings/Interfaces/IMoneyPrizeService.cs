namespace TheDugout.Services.GameSettings.Interfaces
{
    using TheDugout.Models.Common;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Teams;

    public interface IMoneyPrizeService
    {
        Task<MoneyPrize?> GetByCodeAsync(string code);
        Task<FinancialTransaction?> GrantToTeamAsync(GameSave gameSave, string prizeCode, Team team, string? customDescription = null);
    }
}
