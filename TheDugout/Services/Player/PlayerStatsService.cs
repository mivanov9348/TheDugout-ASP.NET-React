namespace TheDugout.Services.Player
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        public async Task<List<PlayerMatchStats>> InitializeMatchStatsAsync(Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Match Fixture is missing.");

            var allPlayers = new List<Models.Players.Player>(22);

            if (match.Fixture.HomeTeam != null)
                allPlayers.AddRange(await _teamPlanService.GetStartingLineupAsync(match.Fixture.HomeTeam, includeDetails: true));

            if (match.Fixture.AwayTeam != null)
                allPlayers.AddRange(await _teamPlanService.GetStartingLineupAsync(match.Fixture.AwayTeam, includeDetails: true));

            if (allPlayers.Count == 0)
                return new List<PlayerMatchStats>();

            var playerIds = allPlayers.Select(p => p.Id).ToHashSet();

            var existingStats = await _context.PlayerMatchStats
                .Where(ps => ps.MatchId == match.Id && playerIds.Contains(ps.PlayerId))
                .ToListAsync();

            var existingPlayerIds = existingStats.Select(ps => ps.PlayerId).ToHashSet();

            var activeSeason = _context.Seasons.FirstOrDefault(x => x.GameSaveId == match.GameSaveId && x.IsActive==true);

            var newStats = allPlayers
                .Where(p => !existingPlayerIds.Contains(p.Id))
                .Select(p => new PlayerMatchStats
                {
                    PlayerId = p.Id,
                    MatchId = match.Id,
                    GameSaveId = match.GameSaveId,
                    CompetitionId = match.CompetitionId,
                    SeasonId = activeSeason.Id,
                    Goals = 0,
                    MatchRating = 0
                })
                .ToList();

            if (newStats.Count > 0)
            {
                await _context.PlayerMatchStats.AddRangeAsync(newStats);
                await _context.SaveChangesAsync();
                existingStats.AddRange(newStats);
            }

            return existingStats;
        }
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

        public async Task<List<PlayerMatchStats>> EnsureMatchStatsAsync(Match match)
        {
            if (match.PlayerStats is { Count: > 0 })
                return match.PlayerStats.ToList();

            match.PlayerStats = await InitializeMatchStatsAsync(match);
            return match.PlayerStats.ToList();
        }
        public async Task UpdateCompetitionStatsAfterMatchAsync(Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Fixture is missing for match.");

            var seasonId = match.Fixture.SeasonId;
            var gameSaveId = match.Fixture.GameSaveId;
            var competitionId = match.CompetitionId;

            var playerIds = match.PlayerStats.Select(ps => ps.PlayerId).ToList();

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

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSeasonStatsAfterMatchAsync(Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Fixture is missing for match.");

            var seasonId = match.Fixture.SeasonId;
            var gameSaveId = match.Fixture.GameSaveId;

            var playerIds = match.PlayerStats.Select(ps => ps.PlayerId).ToList();

            var seasonStats = await _context.PlayerSeasonStats
                .Where(pss => pss.SeasonId == seasonId && playerIds.Contains(pss.PlayerId))
                .ToListAsync();

            foreach (var matchStat in match.PlayerStats)
            {
                // Изчисли рейтинга за конкретния мач
                matchStat.MatchRating = CalculateMatchRating(matchStat);

                var existing = seasonStats.FirstOrDefault(pss => pss.PlayerId == matchStat.PlayerId);

                if (existing == null)
                {
                    existing = new PlayerSeasonStats
                    {
                        PlayerId = matchStat.PlayerId,
                        SeasonId = seasonId,
                        GameSaveId = gameSaveId,
                        MatchesPlayed = 1,
                        Goals = matchStat.Goals,
                        SeasonRating = matchStat.MatchRating
                    };
                    _context.PlayerSeasonStats.Add(existing);
                }
                else
                {
                    existing.MatchesPlayed += 1;
                    existing.Goals += matchStat.Goals;

                    // Изчисли новия сезонен рейтинг на база всички MatchStats на играча
                    var playerMatches = await _context.PlayerMatchStats
                        .Where(pms => pms.PlayerId == matchStat.PlayerId &&
                                      pms.GameSaveId == gameSaveId)
                        .ToListAsync();

                    existing.SeasonRating = CalculateSeasonRating(playerMatches);
                }
            }

            await _context.SaveChangesAsync();
        }
        public double CalculateMatchRating(PlayerMatchStats stat)
        {
            double rating = 5.0;
            rating += stat.Goals * 1.5;
            return Math.Round(Math.Clamp(rating, 1.0, 10.0), 2);
        }

        public double CalculateSeasonRating(IEnumerable<PlayerMatchStats> matchStats)
        {
            if (matchStats == null || !matchStats.Any())
                return 1.0;

            foreach (var stat in matchStats)
            {
                if (stat.MatchRating <= 0)
                    stat.MatchRating = CalculateMatchRating(stat);
            }

            double avg = matchStats.Average(m => m.MatchRating);
            return Math.Round(Math.Clamp(avg, 1.0, 10.0), 2);
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
