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

    }
}
