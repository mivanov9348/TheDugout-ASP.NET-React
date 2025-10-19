namespace TheDugout.Services.Player
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

        /// <summary>
        /// Създава PlayerMatchStats за всички активни играчи в даден мач.
        /// </summary>
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
                    Goals = 0
                });
            }

            if (newStats.Any())
            {
                await _context.PlayerMatchStats.AddRangeAsync(newStats);
                await _context.SaveChangesAsync();
            }

            return existingStats.Concat(newStats).ToList();
        }

        /// <summary>
        /// Обновява PlayerMatchStats въз основа на MatchEvent (пример: гол).
        /// </summary>
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

        /// <summary>
        /// Ако няма PlayerMatchStats за мача, създава ги.
        /// </summary>
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
        
        public async Task UpdateStatsAfterMatchAsync(Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Fixture is missing for match.");

            var seasonId = match.Fixture.SeasonId;
            var gameSaveId = match.Fixture.GameSaveId;
            var competitionId = match.CompetitionId;

            var playerIds = match.PlayerStats.Select(ps => ps.PlayerId).ToList();

            // --- PlayerCompetitionStats ---
            var competitionStats = await _context.PlayerCompetitionStats
                .Where(pcs => pcs.SeasonId == seasonId &&
                              pcs.CompetitionId == competitionId &&
                              playerIds.Contains(pcs.PlayerId))
                .ToListAsync();

            foreach (var matchStat in match.PlayerStats)
            {
                var existing = competitionStats.FirstOrDefault(pcs => pcs.PlayerId == matchStat.PlayerId);

                if (existing == null)
                {
                    existing = new PlayerCompetitionStats
                    {
                        PlayerId = matchStat.PlayerId,
                        CompetitionId = competitionId,
                        SeasonId = seasonId,
                        GameSaveId = gameSaveId,
                        MatchesPlayed = 1,
                        Goals = matchStat.Goals
                    };
                    _context.PlayerCompetitionStats.Add(existing);
                }
                else
                {
                    existing.MatchesPlayed += 1;
                    existing.Goals += matchStat.Goals;
                }
            }

            // --- PlayerSeasonStats ---
            var seasonStats = await _context.PlayerSeasonStats
                .Where(pss => pss.SeasonId == seasonId && playerIds.Contains(pss.PlayerId))
                .ToListAsync();

            foreach (var matchStat in match.PlayerStats)
            {
                var existing = seasonStats.FirstOrDefault(pss => pss.PlayerId == matchStat.PlayerId);

                if (existing == null)
                {
                    existing = new PlayerSeasonStats
                    {
                        PlayerId = matchStat.PlayerId,
                        SeasonId = seasonId,
                        GameSaveId = gameSaveId,
                        MatchesPlayed = 1,
                        Goals = matchStat.Goals
                    };
                    _context.PlayerSeasonStats.Add(existing);
                }
                else
                {
                    existing.MatchesPlayed += 1;
                    existing.Goals += matchStat.Goals;
                }
            }

            await _context.SaveChangesAsync();
        }
 
        public async Task<List<(int CompetitionId, int PlayerId, int Goals)>> GetTopScorersByCompetitionAsync(int seasonId)
        {
            var topScorers = await _context.PlayerCompetitionStats
                .Include(pcs => pcs.Player)
                .Where(pcs => pcs.SeasonId == seasonId)
                .GroupBy(pcs => pcs.CompetitionId)
                .Select(g => g
                    .OrderByDescending(x => x.Goals)
                    .ThenBy(x => x.Player.LastName)
                    .Select(x => new { x.CompetitionId, x.PlayerId, x.Goals })
                    .First())
                .ToListAsync();

            return topScorers
                .Select(x => (x.CompetitionId, x.PlayerId, x.Goals))
                .ToList();
        }
    }
}
