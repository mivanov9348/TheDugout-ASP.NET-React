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
        private readonly ILogger<EndSeasonService> _logger;
        public EndSeasonService(DugoutDbContext context, ICompetitionService competitionService, INewSeasonService seasonGenerationService, IPlayerStatsService playerStatsService, ISeasonCleanupService seasonCleanupService, ILogger<EndSeasonService> logger)
        {
            _context = context;
            _competitionService = competitionService;
            _seasonGenerationService = seasonGenerationService;
            _playerStatsService = playerStatsService;
            _seasonCleanupService = seasonCleanupService;
            _logger = logger;
        }
        public async Task<bool> ProcessSeasonEndAsync(int seasonId)
        {
            _logger.LogInformation("🟢 [ProcessSeasonEndAsync] Start for season {SeasonId}", seasonId);

            var season = await _context.Seasons
                .Include(s => s.Competitions)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null)
            {
                _logger.LogError("❌ [ProcessSeasonEndAsync] Season {SeasonId} not found.", seasonId);
                throw new Exception($"Season {seasonId} not found.");
            }

            _logger.LogInformation("✅ [ProcessSeasonEndAsync] Found season {SeasonId}, IsActive={IsActive}", season.Id, season.IsActive);

            if (!season.IsActive)
            {
                _logger.LogWarning("⚠️ [ProcessSeasonEndAsync] Season {SeasonId} is not active, returning false.", season.Id);
                return false;
            }

            // Check if all competitions are finished
            _logger.LogInformation("📊 [ProcessSeasonEndAsync] Checking if all competitions are finished...");
            var allFinished = await _competitionService.AreAllCompetitionsFinishedAsync(seasonId);
            _logger.LogInformation("📊 [ProcessSeasonEndAsync] All competitions finished = {AllFinished}", allFinished);

            if (!allFinished)
            {
                _logger.LogWarning("⚠️ [ProcessSeasonEndAsync] Not all competitions are finished. Returning false.");
                return false;
            }

            // Generate player and competition stats for the season
            await _competitionService.GenerateSeasonResultAsync(season.Id);

            //// Mark season as ended
            //season.IsActive = false;
            //await _context.SaveChangesAsync();

            _logger.LogInformation("🏁 [ProcessSeasonEndAsync] Season {SeasonId} marked as ended.", seasonId);

            _logger.LogInformation("✅ [ProcessSeasonEndAsync] Returning TRUE for season {SeasonId}", seasonId);
            return true;
        }

        public async Task<bool> StartNewSeasonAsync(int seasonId)
        {
            _logger.LogInformation("🚀 [StartNewSeasonAsync] Starting new season after {SeasonId}", seasonId);

            // Clean up old season data
            await _seasonCleanupService.CleanupOldSeasonDataAsync(seasonId);

            var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);
            if (season == null)
            {
                _logger.LogError("❌ [StartNewSeasonAsync] Season {SeasonId} not found.", seasonId);
                return false;
            }

            // Generate new season
            var nextSeasonStart = season.EndDate.AddDays(1);
            await _seasonGenerationService.GenerateSeason(season.GameSave, nextSeasonStart);

            _logger.LogInformation("✅ [StartNewSeasonAsync] New season successfully generated after {SeasonId}", seasonId);
            return true;
        }




    }
}
