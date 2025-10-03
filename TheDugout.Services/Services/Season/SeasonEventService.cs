
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public class SeasonEventService : ISeasonEventService
    {
        private readonly DugoutDbContext _context;
        public SeasonEventService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task CheckEventsForDayAsync(int seasonId, DateTime date)
        {
            var evt = await _context.SeasonEvents
                .FirstOrDefaultAsync(e => e.SeasonId == seasonId && e.Date.Date == date.Date);

            if (evt == null)
                return; 

            switch (evt.Type)
            {
                case SeasonEventType.TransferWindow:
                    evt.Description = "Transfer Window Active";
                    break;

                case SeasonEventType.ChampionshipMatch:
                    evt.Description = "League Match Played";
                    evt.IsOccupied = true;
                    break;

                case SeasonEventType.CupMatch:
                    evt.Description = "Cup Match Played";
                    evt.IsOccupied = true;
                    break;

                case SeasonEventType.EuropeanMatch:
                    evt.Description = "European Match Played";
                    evt.IsOccupied = true;
                    break;

                case SeasonEventType.Other:
                default:
                    evt.Description = "Rest day";
                    break;
            }

            await _context.SaveChangesAsync();
        }
    }
}
