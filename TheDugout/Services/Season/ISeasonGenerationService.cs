using TheDugout.Models.Game;
using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public interface ISeasonGenerationService
    {
        Models.Seasons.Season GenerateSeason(GameSave gameSave, DateTime startDate);
        DateTime GetNextFreeDate(Models.Seasons.Season season, SeasonEventType type, DateTime fromDate);
        List<DateTime> DistributeRounds(Models.Seasons.Season season, SeasonEventType type, int totalRounds);
        void AssignFixtureDates(
    List<Models.Matches.Fixture> fixtures,
    Models.Seasons.Season season,
    ISeasonGenerationService seasonService,
    DateTime fallbackStartDate);
    }
}