using TheDugout.Models.Game;
using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public class SeasonGenerationService : ISeasonGenerationService
    {
            public Models.Seasons.Season GenerateSeason(GameSave gameSave, DateTime startDate)
            {
                var season = new Models.Seasons.Season
                {
                    GameSave = gameSave,
                    StartDate = startDate,
                    EndDate = startDate.AddYears(1).AddDays(-1),
                    CurrentDate = startDate
                };

                var events = new List<SeasonEvent>();
                var currentDate = season.StartDate;

                while (currentDate <= season.EndDate)
                {
                    var evt = new SeasonEvent
                    {
                        Season = season,
                        Date = currentDate,
                        Type = GetEventType(currentDate, season.StartDate),
                        Description = GetDescription(currentDate, season.StartDate),
                        IsOccupied = false
                    };

                    events.Add(evt);
                    currentDate = currentDate.AddDays(1);
                }

                season.Events = events;
                return season;
            }

            private SeasonEventType GetEventType(DateTime date, DateTime seasonStart)
            {
                // първите 7 дни от сезона са трансферен прозорец
                if (date >= seasonStart && date < seasonStart.AddDays(7))
                    return SeasonEventType.TransferWindow;

                return date.DayOfWeek switch
                {
                    DayOfWeek.Saturday => SeasonEventType.ChampionshipMatch,
                    DayOfWeek.Thursday => SeasonEventType.CupMatch,
                    DayOfWeek.Tuesday => SeasonEventType.EuropeanMatch,
                    _ => SeasonEventType.Other
                };
            }

            private string GetDescription(DateTime date, DateTime seasonStart) =>
                GetEventType(date, seasonStart) switch
                {
                    SeasonEventType.TransferWindow => "Transfer Window",
                    SeasonEventType.ChampionshipMatch => "League Matchday",
                    SeasonEventType.CupMatch => "Cup Match",
                    SeasonEventType.EuropeanMatch => "European Match",
                    _ => "Free day"
                };

        public DateTime GetNextFreeDate(Models.Seasons.Season season, SeasonEventType type, DateTime fromDate)
        {
            return season.Events
                .Where(e => !e.IsOccupied && e.Type == type && e.Date >= fromDate)
                .OrderBy(e => e.Date)
                .Select(e => e.Date)
                .FirstOrDefault();
        }

        public List<DateTime> DistributeRounds(Models.Seasons.Season season, SeasonEventType type, int totalRounds)
        {
            var freeDates = season.Events
                .Where(e => !e.IsOccupied && e.Type == type)
                .OrderBy(e => e.Date)
                .Select(e => e.Date)
                .ToList();

            if (!freeDates.Any() || totalRounds <= 0)
                return new List<DateTime>();

            var result = new List<DateTime>();
            double step = (double)freeDates.Count / totalRounds;

            for (int i = 0; i < totalRounds; i++)
            {
                int index = (int)Math.Round(i * step);
                if (index >= freeDates.Count)
                    index = freeDates.Count - 1;

                result.Add(freeDates[index]);
            }

            return result.Distinct().ToList();
        }

        public void AssignFixtureDates(
    List<Models.Matches.Fixture> fixtures,
    Models.Seasons.Season season,
    ISeasonGenerationService seasonService,
    DateTime fallbackStartDate)
        {
            var totalRounds = fixtures.Max(f => f.Round);
            var roundDates = seasonService
                .DistributeRounds(season, SeasonEventType.ChampionshipMatch, totalRounds);

            for (int round = 1; round <= totalRounds; round++)
            {
                var date = round <= roundDates.Count
                    ? roundDates[round - 1]
                    : fallbackStartDate.AddDays(7 * (round - 1));

                foreach (var fixture in fixtures.Where(f => f.Round == round))
                {
                    fixture.Date = date;
                }
            }
        }

    }
}
