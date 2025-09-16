using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public interface ISeasonCalendarService
    {
        DateTime GetNextFreeDate(Models.Seasons.Season season, SeasonEventType type, DateTime fromDate);
        List<DateTime> DistributeRounds(Models.Seasons.Season season, SeasonEventType type, int totalRounds, int interval = 1);

        List<DateTime> DistributeLeagueRounds(Models.Seasons.Season season, int totalRounds);

        List<DateTime> DistributeCupRounds(Models.Seasons.Season season, int totalRounds);

        List<DateTime> DistributeEuropeanRounds(Models.Seasons.Season season, int totalRounds);
    }
}
