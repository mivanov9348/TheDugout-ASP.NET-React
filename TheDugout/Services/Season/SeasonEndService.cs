namespace TheDugout.Services.Season
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Player;
    using TheDugout.Services.Season.Interfaces;
    public class SeasonEndService : ISeasonEndService
    {
        private readonly DugoutDbContext _context;
        private readonly ICompetitionService _competitionService;
        private readonly ISeasonGenerationService _seasonGenerationService;
        private readonly IPlayerStatsService _playerStatsService;
        public SeasonEndService(DugoutDbContext context, ICompetitionService competitionService, ISeasonGenerationService seasonGenerationService, IPlayerStatsService playerStatsService)
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

            if (season == null) throw new Exception($"Season {seasonId} not found.");
            if (!season.IsActive) return false;

            var allFinished = await _competitionService.AreAllCompetitionsFinishedAsync(seasonId);
            if (!allFinished) return false;

            var topScorers = await _playerStatsService.GetTopScorersByCompetitionAsync(seasonId);
          
            foreach (var competition in season.Competitions)
            {
                await _competitionService.GenerateSeasonResultAsync(season.Id);
            }

            await _competitionService.ProcessPromotionAndRelegationAsync(seasonId);

            season.IsActive = false;
            await _context.SaveChangesAsync();

            var nextSeasonStart = season.EndDate.AddDays(1);
            await _seasonGenerationService.GenerateSeason(season.GameSave, nextSeasonStart);

            return true;
        }

    }
}
