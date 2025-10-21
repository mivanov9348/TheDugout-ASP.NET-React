namespace TheDugout.Services.Season
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Season.Interfaces;
    public class EndSeasonService : IEndSeasonService
    {
        private readonly DugoutDbContext _context;
        private readonly ICompetitionService _competitionService;
        private readonly INewSeasonService _seasonGenerationService;
        private readonly IPlayerStatsService _playerStatsService;
        private readonly ISeasonCleanupService _seasonCleanupService;
        public EndSeasonService(DugoutDbContext context, ICompetitionService competitionService, INewSeasonService seasonGenerationService, IPlayerStatsService playerStatsService, ISeasonCleanupService seasonCleanupService)
        {
            _context = context;
            _competitionService = competitionService;
            _seasonGenerationService = seasonGenerationService;
            _playerStatsService = playerStatsService;
            _seasonCleanupService = seasonCleanupService;
        }
        public async Task<bool> ProcessSeasonEndAsync(int seasonId)
        {
            Console.WriteLine($"🟢 [ProcessSeasonEndAsync] Start for season {seasonId}");

            var season = await _context.Seasons
                .Include(s => s.Competitions)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null)
            {
                Console.WriteLine($"❌ [ProcessSeasonEndAsync] Season {seasonId} not found.");
                throw new Exception($"Season {seasonId} not found.");
            }

            Console.WriteLine($"✅ [ProcessSeasonEndAsync] Found season {season.Id}, IsActive={season.IsActive}");

            if (!season.IsActive)
            {
                Console.WriteLine($"⚠️ [ProcessSeasonEndAsync] Season {season.Id} is not active, returning false.");
                return false;
            }

            // Check if all competitions are finished
            Console.WriteLine($"📊 [ProcessSeasonEndAsync] Checking if all competitions are finished...");
            var allFinished = await _competitionService.AreAllCompetitionsFinishedAsync(seasonId);
            Console.WriteLine($"📊 [ProcessSeasonEndAsync] All competitions finished = {allFinished}");

            if (!allFinished)
            {
                Console.WriteLine($"⚠️ [ProcessSeasonEndAsync] Not all competitions are finished. Returning false.");
                return false;
            }

            // Generate player stats for the season
            await _competitionService.GenerateSeasonResultAsync(season.Id);

            // Season cleanup
            await _seasonCleanupService.CleanupOldSeasonDataAsync(season.Id);

            // End the season
            season.IsActive = false;
            await _context.SaveChangesAsync();

            // Generate the next season
            var nextSeasonStart = season.EndDate.AddDays(1);
            await _seasonGenerationService.GenerateSeason(season.GameSave, nextSeasonStart);

            Console.WriteLine($"✅ [ProcessSeasonEndAsync] Returning TRUE for season {seasonId}");
            return true;
        }

    }
}
