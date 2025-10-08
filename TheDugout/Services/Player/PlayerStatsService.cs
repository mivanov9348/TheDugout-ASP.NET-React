using Microsoft.EntityFrameworkCore;
using System;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Models.Common;
using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Player
{
    public class PlayerStatsService : IPlayerStatsService
    {
        private readonly DugoutDbContext _context;

        public PlayerStatsService(DugoutDbContext context)
        {
            _context = context;
        }

        /// Create PlayerMatchStats for all active players in the match
        public async Task<List<PlayerMatchStats>> InitializeMatchStatsAsync(Models.Matches.Match match)
        {
            if (match.Fixture == null)
                throw new InvalidOperationException("Match Fixture is missing.");

            var compType = match.Fixture.CompetitionType;
            var allPlayers = new List<Models.Players.Player>();

            if (match.Fixture.HomeTeam?.Players != null)
                allPlayers.AddRange(match.Fixture.HomeTeam.Players.Where(p => p.IsActive));

            if (match.Fixture.AwayTeam?.Players != null)
                allPlayers.AddRange(match.Fixture.AwayTeam.Players.Where(p => p.IsActive));

            if (!allPlayers.Any())
                return new List<PlayerMatchStats>();

            var playerIds = allPlayers.Select(p => p.Id).ToList();

            // Вземаме вече записаните PlayerMatchStats (ако има)
            var existingStats = await _context.PlayerMatchStats
                .Where(ps => ps.MatchId == match.Id && playerIds.Contains(ps.PlayerId))
                .ToListAsync();

            var newStats = new List<PlayerMatchStats>();

            // Добавяме само липсващите записи
            foreach (var player in allPlayers)
            {
                if (existingStats.Any(ps => ps.PlayerId == player.Id))
                    continue;

                newStats.Add(new PlayerMatchStats
                {
                    PlayerId = player.Id,
                    MatchId = match.Id,
                    CompetitionId = match.CompetitionId,
                    Goals = 0,
                });
            }

            if (newStats.Count > 0)
            {
                await _context.PlayerMatchStats.AddRangeAsync(newStats);
                await _context.SaveChangesAsync();
            }

            // Връщаме комбинирания списък (нови + вече съществуващи)
            return existingStats.Concat(newStats).ToList();
        }

        /// Update PlayerMatchStats based on a MatchEvent
        public void UpdateStats(MatchEvent matchEvent, PlayerMatchStats stats)
        {
            switch (matchEvent.EventType.Code)
            {
                case "SHT": // Shot
                    if (matchEvent.Outcome.Name == "Goal")
                        stats.Goals++;
                    break;
                    // Тук можеш да добавиш и други събития (асистенции, пасове, дрибъл и т.н.)
            }
        }

        /// Check if PlayerMatchStats exist for the match; if not, initialize them
        public async Task<List<PlayerMatchStats>> EnsureMatchStatsAsync(Models.Matches.Match match)
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
        public async Task UpdateSeasonStatsAfterMatchAsync(Models.Matches.Match match)
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
                             playerIds.Contains(ps.PlayerId))
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

    }
}
