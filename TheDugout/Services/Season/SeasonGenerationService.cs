using TheDugout.Models;

namespace TheDugout.Services.Season
{
    public class SeasonGenerationService : ISeasonGenerationService
    {
        public Models.Season GenerateSeason(GameSave gameSave, DateTime startDate)
        {
            var season = new Models.Season
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
                    Type = GetEventType(currentDate),
                    Description = GetDescription(currentDate)
                };

                events.Add(evt);
                currentDate = currentDate.AddDays(1);
            }

            season.Events = events;
            return season;
        }

        private SeasonEventType GetEventType(DateTime date)
        {
            if ((date.Month == 7 && date.Day <= 10) ||
                (date.Month == 1 && date.Day <= 10))
                return SeasonEventType.TransferWindow;

            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => SeasonEventType.ChampionshipMatch,
                DayOfWeek.Thursday => SeasonEventType.CupMatch,
                DayOfWeek.Tuesday => SeasonEventType.EuropeanMatch,
                _ => SeasonEventType.Other
            };
        }

        private string GetDescription(DateTime date) =>
            GetEventType(date) switch
            {
                SeasonEventType.TransferWindow => "Transfer Window",
                SeasonEventType.ChampionshipMatch => "League Matchday",
                SeasonEventType.CupMatch => "Cup Match",
                SeasonEventType.EuropeanMatch => "European Match",
                _ => "Free day"
            };
    }
}
