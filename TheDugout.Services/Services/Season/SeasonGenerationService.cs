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
                CurrentDate = startDate,
                IsActive = true
            };

            var events = new List<SeasonEvent>();
            var currentDate = season.StartDate;

            while (currentDate <= season.EndDate)
            {
                var evt = new SeasonEvent
                {
                    Season = season,
                    Date = currentDate,
                    Type = GetEventType(currentDate, season.StartDate, season.EndDate),
                    Description = GetDescription(currentDate, season.StartDate, season.EndDate),
                    IsOccupied = false
                };

                events.Add(evt);
                currentDate = currentDate.AddDays(1);
            }

            season.Events = events;
            return season;
        }

        private SeasonEventType GetEventType(DateTime date, DateTime seasonStart, DateTime seasonEnd)
        {
            // първите 7 дни трансферен прозорец
            if (date >= seasonStart && date < seasonStart.AddDays(7))
                return SeasonEventType.TransferWindow;

            // средата на сезона = 7 дни трансферен прозорец
            var midSeason = seasonStart.AddDays((seasonEnd - seasonStart).Days / 2);
            if (date >= midSeason && date < midSeason.AddDays(7))
                return SeasonEventType.TransferWindow;

            // седмични събития
            return date.DayOfWeek switch
            {
                DayOfWeek.Tuesday => SeasonEventType.EuropeanMatch,
                DayOfWeek.Thursday => SeasonEventType.CupMatch,
                DayOfWeek.Saturday => SeasonEventType.ChampionshipMatch,
                _ => SeasonEventType.TrainingDay
            };
        }

        private string GetDescription(DateTime date, DateTime seasonStart, DateTime seasonEnd) =>
            GetEventType(date, seasonStart, seasonEnd) switch
            {
                SeasonEventType.TransferWindow => "Transfer Window",
                SeasonEventType.ChampionshipMatch => "League Matchday",
                SeasonEventType.CupMatch => "Cup Match",
                SeasonEventType.EuropeanMatch => "European Match",
                SeasonEventType.TrainingDay => "Training Day",
                _ => "Other"
            };
    }
}
