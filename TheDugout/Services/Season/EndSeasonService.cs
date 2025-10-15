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
        public EndSeasonService(DugoutDbContext context, ICompetitionService competitionService, INewSeasonService seasonGenerationService, IPlayerStatsService playerStatsService)
        {
            _context = context;
            _competitionService = competitionService;
            _seasonGenerationService = seasonGenerationService;
            _playerStatsService = playerStatsService;
        }
        public async Task<bool> ProcessSeasonEndAsync(int seasonId)
        {
            var season = await _context.Seasons
                .Include(s => s.Competitions)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null)
                throw new Exception($"Season {seasonId} not found.");

            if (!season.IsActive)
                return false;

            // Check if all competitions are finished
            var allFinished = await _competitionService.AreAllCompetitionsFinishedAsync(seasonId);
            if (!allFinished)
                return false;

            // Generate player stats for the season
            await _competitionService.GenerateSeasonResultAsync(season.Id);

            // End the season
            season.IsActive = false;
            await _context.SaveChangesAsync();

            // Generate the next season
            var nextSeasonStart = season.EndDate.AddDays(1);
            await _seasonGenerationService.GenerateSeason(season.GameSave, nextSeasonStart);

            return true;
        }

    }
}
