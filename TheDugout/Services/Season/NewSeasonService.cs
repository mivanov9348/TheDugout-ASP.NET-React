namespace TheDugout.Services.Season
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    using TheDugout.Services.Season.Interfaces;

    public class NewSeasonService : INewSeasonService
    {
        private readonly DugoutDbContext _context;
        public NewSeasonService(DugoutDbContext context)
        {
            _context = context;
        }
        public async Task<Season> GenerateSeason(GameSave gameSave, DateTime startDate)
        {
            var season = new Season
            {
                GameSaveId = gameSave.Id,
                StartDate = startDate,
                EndDate = startDate.AddYears(1).AddDays(-1),
                CurrentDate = startDate,
                IsActive = true
            };

            // Save the new season to get its ID
            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

            // Update the GameSave with the new season ID
            gameSave.CurrentSeasonId = season.Id;
            await _context.SaveChangesAsync();

            // Create season events
            var events = new List<SeasonEvent>();
            var currentDate = season.StartDate;

            while (currentDate <= season.EndDate)
            {
                events.Add(new SeasonEvent
                {
                    SeasonId = season.Id,
                    Date = currentDate,
                    Type = GetEventType(currentDate, season.StartDate, season.EndDate),
                    Description = GetDescription(currentDate, season.StartDate, season.EndDate),
                    GameSaveId = gameSave.Id,
                    IsOccupied = false
                });
                currentDate = currentDate.AddDays(1);
            }

            _context.SeasonEvents.AddRange(events);
            await _context.SaveChangesAsync();

            return season;
        }
        private SeasonEventType GetEventType(DateTime date, DateTime seasonStart, DateTime seasonEnd)
        {
            if (date.Date == seasonStart.Date)
                return SeasonEventType.StartSeason;

            if (date.Date == seasonEnd.Date)
                return SeasonEventType.EndOfSeason;

            // First 7 days of the season = transfer window
            if (date >= seasonStart && date < seasonStart.AddDays(7))
                return SeasonEventType.TransferWindow;

            // Middle 7 days of the season = transfer window
            var midSeason = seasonStart.AddDays((seasonEnd - seasonStart).Days / 2);
            if (date >= midSeason && date < midSeason.AddDays(7))
                return SeasonEventType.TransferWindow;

            // Weekly events
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
                SeasonEventType.StartSeason => "Start of New Season",
                SeasonEventType.EndOfSeason => "End of the Season",
                SeasonEventType.TransferWindow => "Transfer Window",
                SeasonEventType.ChampionshipMatch => "League Matchday",
                SeasonEventType.CupMatch => "Cup Match",
                SeasonEventType.EuropeanMatch => "European Match",
                SeasonEventType.TrainingDay => "Training Day",
                _ => "Other"
            };
    }
}
