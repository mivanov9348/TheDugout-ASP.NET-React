namespace TheDugout.Services.Season.Interfaces
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    public interface INewSeasonService
    {
        Task<Season> GenerateSeason(GameSave gameSave, DateTime startDate);
        Task<bool> StartNewSeasonAsync(int seasonId);
        Task<Season> GetActiveSeason(int gameSaveId);
    }
}