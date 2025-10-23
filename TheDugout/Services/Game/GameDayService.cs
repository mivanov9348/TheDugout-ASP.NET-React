namespace TheDugout.Services.Game
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Seasons;
    using TheDugout.Services.CPUManager;
    using TheDugout.Services.Game.Interfaces;
    using TheDugout.Services.Season.Interfaces;

    public class GameDayService : IGameDayService
    {
        private readonly DugoutDbContext _context;
        private readonly ISeasonEventService _seasonEventService;
        private readonly ICPUManagerService _cpuManagerService; 

        public GameDayService(DugoutDbContext context, ISeasonEventService seasonEventService, ICPUManagerService cpuManagerService)
        {
            _context = context;
            _seasonEventService = seasonEventService;
            _cpuManagerService = cpuManagerService;
        }
        public async Task ProcessNextDayAsync(int saveId, Func<string, Task>? progress = null)
        {
            var save = await _context.GameSaves
                .Include(s => s.Seasons)
                    .ThenInclude(se => se.Events)
                .FirstOrDefaultAsync(s => s.Id == saveId);

            if (save == null) throw new Exception($"GameSave {saveId} not found.");

            var season = save.Seasons.OrderByDescending(s => s.StartDate).FirstOrDefault();
            if (season == null) throw new Exception("No active season found.");

            season.CurrentDate = season.CurrentDate.AddDays(1);
            await _context.SaveChangesAsync();

            if (progress != null) await progress("⏩ Date advanced to " + season.CurrentDate.ToShortDateString());

            // CPU logic
            await _cpuManagerService.RunDailyCpuLogicAsync(
                save.Id, season.Id, season.CurrentDate, save.UserTeamId, progress
            );

            var todaysEvent = season.Events
                .FirstOrDefault(e => e.Date.Date == season.CurrentDate.Date);

            if (todaysEvent != null)
            {
                switch (todaysEvent.Type)
                {
                    case SeasonEventType.ChampionshipMatch:
                        save.NextDayActionLabel = "Play League Match";
                        break;
                    case SeasonEventType.CupMatch:
                        save.NextDayActionLabel = "Play Cup Match";
                        break;
                    case SeasonEventType.EuropeanMatch:
                        save.NextDayActionLabel = "Play European Match";
                        break;
                    default:
                        save.NextDayActionLabel = "Next Day →";
                        break;
                }
            }
            else
            {
                save.NextDayActionLabel = "Next Day →";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<object> ProcessNextDayAndGetResultAsync(int saveId)
        {
            await ProcessNextDayAsync(saveId);

            var updatedSave = await _context.GameSaves
                .AsSplitQuery()
                .Include(gs => gs.UserTeam).ThenInclude(t => t.Country)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Country)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Template)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Country)
                .Include(gs => gs.Seasons).ThenInclude(s => s.Events)
                .Include(gs => gs.Seasons).ThenInclude(s => s.Fixtures)
                .FirstAsync(gs => gs.Id == saveId);

            var today = updatedSave.Seasons.First().CurrentDate.Date;

            var todaysFixtures = updatedSave.Seasons
                .SelectMany(s => s.Fixtures)
                .Where(f => f.Date.Date == today)
                .ToList();

            return new
            {
                GameSave = updatedSave.ToDto(),
                HasMatchesToday = todaysFixtures.Any(),
                HasUnplayedMatchesToday = todaysFixtures.Any(f => f.Status != MatchStageEnum.Played)
            };
        }

    }
}
