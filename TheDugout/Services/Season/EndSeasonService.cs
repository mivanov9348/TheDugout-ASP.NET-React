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
        private readonly ILogger<EndSeasonService> _logger;
        public EndSeasonService(DugoutDbContext context, ICompetitionService competitionService, ILogger<EndSeasonService> logger)
        {
            _context = context;
            _competitionService = competitionService;
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

            // Mark season as ended
            season.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("🏁 [ProcessSeasonEndAsync] Season {SeasonId} marked as ended.", seasonId);

            _logger.LogInformation("✅ [ProcessSeasonEndAsync] Returning TRUE for season {SeasonId}", seasonId);
            return true;
        }    
    }
}
