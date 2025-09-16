using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public class SeasonSchedulingService : ISeasonSchedulingService
    {
        private readonly ISeasonCalendarService _seasonCalendarService;
        public SeasonSchedulingService(ISeasonCalendarService seasonCalendarService)
        {
            _seasonCalendarService = seasonCalendarService;
        }

        public void AssignFixtureDates(
    List<Models.Matches.Fixture> fixtures,
    Models.Seasons.Season season,
    DateTime fallbackStartDate)
        {
            var totalRounds = fixtures.Max(f => f.Round);

            // Тук вече директно си взимаш равномерно разпределени дати
            var roundDates = _seasonCalendarService.DistributeRounds(
                season,
                SeasonEventType.ChampionshipMatch,
                totalRounds
            );

            for (int round = 1; round <= totalRounds; round++)
            {
                DateTime date;
                if (round <= roundDates.Count)
                {
                    date = roundDates[round - 1];
                }
                else
                {
                    date = fallbackStartDate.AddDays(7 * (round - 1));
                }

                var seasonEvent = season.Events
                    .FirstOrDefault(e => e.Date == date && e.Type == SeasonEventType.ChampionshipMatch);

                if (seasonEvent != null && !seasonEvent.IsOccupied)
                {
                    seasonEvent.IsOccupied = true;
                }

                foreach (var fixture in fixtures.Where(f => f.Round == round))
                {
                    fixture.Date = date;
                }
            }
        }

    }
}
