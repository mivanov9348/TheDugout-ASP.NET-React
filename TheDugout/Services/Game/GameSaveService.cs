using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services.League;
using TheDugout.Services.Season;

namespace TheDugout.Services.Game
{
    public class GameSaveService : IGameSaveService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameSaveService> _logger;
        private readonly ILeagueService _leagueGenerator;
        private readonly ISeasonGenerationService _seasonGenerator;

        public GameSaveService(
            DugoutDbContext context,
            ILogger<GameSaveService> logger,
            ILeagueService leagueGenerator,
            ISeasonGenerationService seasonGenerator
        )
        {
            _context = context;
            _logger = logger;
            _leagueGenerator = leagueGenerator;
            _seasonGenerator = seasonGenerator;
        }

        public async Task<List<object>> GetUserSavesAsync(int userId)
        {
            return await _context.GameSaves
                .AsNoTracking()
                .Where(gs => gs.UserId == userId)
                .OrderByDescending(gs => gs.CreatedAt)
                .Select(gs => new { gs.Id, gs.Name, gs.CreatedAt })
                .ToListAsync<object>();
        }

        public async Task<GameSave?> GetGameSaveAsync(int userId, int saveId)
        {
            return await _context.GameSaves
                .Include(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Players)
                .Include(gs => gs.Seasons).ThenInclude(s => s.Events)
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId);
        }

        public async Task<bool> DeleteGameSaveAsync(int userId, int saveId)
        {
            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId);

            if (gameSave == null) return false;

            _context.GameSaves.Remove(gameSave);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GameSave> StartNewGameAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid userId.");

            var saveCount = await _context.GameSaves.CountAsync(gs => gs.UserId == userId);
            if (saveCount >= 3)
                throw new InvalidOperationException("3 saves max!");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var gameSave = new GameSave
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                };

                // Start new season
                var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
                var season = _seasonGenerator.GenerateSeason(gameSave, startDate);
                gameSave.Seasons.Add(season);

                // League and teams 
                var leagues = await _leagueGenerator.GenerateLeaguesAsync(gameSave);
                foreach (var league in leagues)
                    gameSave.Leagues.Add(league);

                _context.GameSaves.Add(gameSave);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await _context.GameSaves
                    .Include(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Players)
                    .Include(gs => gs.Seasons).ThenInclude(s => s.Events)
                    .FirstAsync(gs => gs.Id == gameSave.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
