using TheDugout.Models;

namespace TheDugout.Services.Season
{
    public interface ISeasonGenerationService
    {
        Models.Season GenerateSeason(GameSave gameSave, DateTime startDate);
    }
}