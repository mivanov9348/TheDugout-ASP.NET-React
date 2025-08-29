using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Data.DtoNewGame;
using TheDugout.Models;

namespace TheDugout.Services.Game
{
    public class GameSaveService : IGameSaveService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameSaveService> _logger;

        public GameSaveService(DugoutDbContext context, ILogger<GameSaveService> logger)
        {
            _context = context;
            _logger = logger;
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
                .Include(gs => gs.Leagues).ThenInclude(l => l.Teams)
                .Include(gs => gs.Seasons)
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId);
        }

        public async Task<bool> DeleteGameSaveAsync(int userId, int saveId)
        {
            var gameSave = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.Teams)
                .Include(gs => gs.Players)
                .Include(gs => gs.Leagues)
                .Include(gs => gs.Messages)
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId);

            if (gameSave == null) return false;

            _context.GameSaves.Remove(gameSave);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GameSave> StartNewGameAsync(int userId, NewGameRequest req)
        {
            var saveCount = await _context.GameSaves.CountAsync(gs => gs.UserId == userId);
            if (saveCount >= 3)
                throw new InvalidOperationException("3 saves Max!");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var gameSave = new GameSave
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                };

                var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
                var season = new Season
                {
                    GameSave = gameSave,
                    StartDate = startDate,
                    EndDate = startDate.AddYears(1).AddDays(-1),
                    CurrentDate = startDate
                };
                gameSave.Seasons.Add(season);

                var leagueTemplates = await _context.LeagueTemplates
                    .Include(lt => lt.TeamTemplates)
                    .ToListAsync();

                foreach (var lt in leagueTemplates)
                {
                    var league = new League
                    {
                        TemplateId = lt.Id,
                        GameSave = gameSave,
                        CountryId = lt.CountryId,
                        Tier = lt.Tier,
                        TeamsCount = lt.TeamsCount,
                        RelegationSpots = lt.RelegationSpots,
                        PromotionSpots = lt.PromotionSpots
                    };

                    foreach (var tt in lt.TeamTemplates)
                    {
                        var team = new Models.Team
                        {
                            TemplateId = tt.Id,
                            GameSave = gameSave,
                            League = league,
                            Name = tt.Name,
                            Abbreviation = tt.Abbreviation,
                            CountryId = tt.CountryId
                        };

                        league.Teams.Add(team);
                        gameSave.Teams.Add(team);
                    }

                    gameSave.Leagues.Add(league);
                }

                _context.GameSaves.Add(gameSave);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await _context.GameSaves
                    .Include(gs => gs.Leagues).ThenInclude(l => l.Teams)
                    .Include(gs => gs.Seasons)
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
