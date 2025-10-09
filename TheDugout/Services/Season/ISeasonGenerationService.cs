using TheDugout.Models.Game;

namespace TheDugout.Services.Season
{
    public interface ISeasonGenerationService
    {
        Task<Models.Seasons.Season> GenerateSeason(GameSave gameSave, DateTime startDate);
    }
}