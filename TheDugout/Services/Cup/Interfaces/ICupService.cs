namespace TheDugout.Services.Cup.Interfaces
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    public interface ICupService
    {
        Task InitializeCupsForGameSaveAsync(GameSave gameSave, int seasonId);
        Task<bool> IsCupFinishedAsync(int cupId);
    }
}
