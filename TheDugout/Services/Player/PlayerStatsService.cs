namespace TheDugout.Services.Player
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using TheDugout.Data;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Team.Interfaces;

    public class PlayerStatsService : IPlayerStatsService
    {
        private readonly DugoutDbContext _context;
        private readonly ITeamPlanService _teamPlanService;

        public PlayerStatsService(DugoutDbContext context, ITeamPlanService teamPlanService)
        {
            _context = context;
            _teamPlanService = teamPlanService;
        }

        /// Create PlayerMatchStats for all active players in the match
        public async Task<List<PlayerMatchStats>> InitializeMatchStatsAsync(Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Match Fixture is missing.");

            var allPlayers = new List<Models.Players.Player>();

            // Вземаме стартовите състави и за двата отбора
            if (match.Fixture.HomeTeam != null)
            {
                var homeLineup = await _teamPlanService.GetStartingLineupAsync(match.Fixture.HomeTeam, includeDetails: true);
                allPlayers.AddRange(homeLineup);
            }

            if (match.Fixture.AwayTeam != null)
            {
                var awayLineup = await _teamPlanService.GetStartingLineupAsync(match.Fixture.AwayTeam, includeDetails: true);
                allPlayers.AddRange(awayLineup);
            }

            if (!allPlayers.Any())
                return new List<PlayerMatchStats>();

            var playerIds = allPlayers.Select(p => p.Id).ToList();

            // Проверяваме дали вече има статистики за тези играчи
            var existingStats = await _context.PlayerMatchStats
                .Where(ps => ps.MatchId == match.Id && playerIds.Contains(ps.PlayerId))
                .ToListAsync();

            var newStats = new List<PlayerMatchStats>();

            // Добавяме само липсващите
            foreach (var player in allPlayers)
            {
                if (existingStats.Any(ps => ps.PlayerId == player.Id))
                    continue;

                newStats.Add(new PlayerMatchStats
                {
                    PlayerId = player.Id,
                    MatchId = match.Id,
                    GameSaveId = match.GameSaveId,
                    CompetitionId = match.CompetitionId,
                    Goals = 0,
                });
            }

            if (newStats.Count > 0)
            {
                await _context.PlayerMatchStats.AddRangeAsync(newStats);
                await _context.SaveChangesAsync();
            }

            return existingStats.Concat(newStats).ToList();
        }

        /// Update PlayerMatchStats based on a MatchEvent
        public void UpdateStats(MatchEvent matchEvent, PlayerMatchStats stats)
        {
            if (matchEvent.EventTypeId == 0)
                return;

            var code = _context.EventTypes
                .Where(e => e.Id == matchEvent.EventTypeId)
                .Select(e => e.Code)
                .FirstOrDefault();

            if (code == null)
                return;

            switch (code)
            {
                case "SHT":
                    if (matchEvent.Outcome?.Name == "Goal")
                        stats.Goals++;
                    break;
            }
        }

        /// Check if PlayerMatchStats exist for the match; if not, initialize them
        public async Task<List<PlayerMatchStats>> EnsureMatchStatsAsync(Match match)
        {
            if (match.PlayerStats == null || !match.PlayerStats.Any())
            {
                var stats = await InitializeMatchStatsAsync(match);
                match.PlayerStats = stats;
                return stats;
            }

            return match.PlayerStats.ToList();
        }

        /// After the match, update PlayerSeasonStats based on PlayerMatchStats
        public async Task UpdateSeasonStatsAfterMatchAsync(Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Fixture is missing for match.");

            var seasonId = match.Fixture.SeasonId;
            var gameSaveId = match.Fixture.GameSaveId;

            // Get Competition (competition)
            var competition = await _context.Competitions
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.SeasonId == seasonId &&
                    c.Type == match.Fixture.CompetitionType);

            if (competition == null)
                throw new InvalidOperationException("Competition not found for match.");

            // Get all player IDs from the match
            var playerIds = match.PlayerStats.Select(ps => ps.PlayerId).ToList();

            // Load existing PlayerSeasonStats for these players in this season and competition
            var existingStats = await _context.PlayerSeasonStats
                .Where(ps => ps.SeasonId == seasonId &&
                             ps.CompetitionId == competition.Id &&
                             playerIds.Contains(ps.PlayerId ?? -1))
                .ToListAsync();

            // Update or create PlayerSeasonStats
            foreach (var playerStat in match.PlayerStats)
            {
                var existing = existingStats.FirstOrDefault(ps => ps.PlayerId == playerStat.PlayerId);

                if (existing == null)
                {
                    existing = new PlayerSeasonStats
                    {
                        PlayerId = playerStat.PlayerId,
                        SeasonId = seasonId,
                        CompetitionId = competition.Id,
                        GameSaveId = gameSaveId,
                        MatchesPlayed = 1,
                        Goals = playerStat.Goals
                    };
                    _context.PlayerSeasonStats.Add(existing);
                }
                else
                {
                    existing.MatchesPlayed += 1;
                    existing.Goals += playerStat.Goals;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<List<(int CompetitionId, int PlayerId, int Goals)>> GetTopScorersByCompetitionAsync(int seasonId)
        {
            var topScorers = await _context.PlayerSeasonStats
                .Include(p => p.Player)
                .Where(p => p.SeasonId == seasonId && p.CompetitionId != null)
                .GroupBy(p => p.CompetitionId!.Value)
                .Select(g => g
                    .OrderByDescending(x => x.Goals)
                    .ThenBy(x => x.Player!.LastName)
                    .Select(x => new { x.CompetitionId, x.PlayerId, x.Goals })
                    .First())
                .ToListAsync();

            return topScorers
                .Select(x => (x.CompetitionId!.Value, x.PlayerId!.Value, x.Goals))
                .ToList();
        }

    }
}
