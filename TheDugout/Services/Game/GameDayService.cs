using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Services.Season;

namespace TheDugout.Services.Game
{
    public class GameDayService : IGameDayService
    {
        private readonly DugoutDbContext _context;
        private readonly ISeasonEventService _seasonEventService;

        public GameDayService(DugoutDbContext context, ISeasonEventService seasonEventService)
        {
            _context = context;
            _seasonEventService = seasonEventService;
        }
        public async Task ProcessNextDayAsync(int saveId)
        {
            var save = await _context.GameSaves
                .Include(s => s.Seasons)
                .FirstOrDefaultAsync(s => s.Id == saveId);

            if (save == null) throw new Exception($"GameSave {saveId} not found.");

            var season = save.Seasons.OrderByDescending(s => s.StartDate).FirstOrDefault();
            if (season == null) throw new Exception("No active season found.");

            season.CurrentDate = season.CurrentDate.AddDays(1);

            await _context.SaveChangesAsync();
        }
    }
}
