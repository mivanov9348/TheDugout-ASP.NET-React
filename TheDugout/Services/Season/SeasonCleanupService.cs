namespace TheDugout.Services.Season
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using TheDugout.Data;
    using TheDugout.Services.Season.Interfaces;
    public class SeasonCleanupService : ISeasonCleanupService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<SeasonCleanupService> _logger;

        public SeasonCleanupService(DugoutDbContext context, ILogger<SeasonCleanupService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task CleanupOldSeasonDataAsync(int seasonId)
        {
            _logger.LogInformation("🧹 Starting cleanup for season {SeasonId}", seasonId);

            await CleanupFixturesAndMatchesAsync(seasonId);
            await CleanupPlayerMatchStatsAsync(seasonId);
            await CleanupTrainingSessionsAsync(seasonId);
            await CleanupTransfersAsync(seasonId);
            await CleanupFreeAgentsAsync(seasonId);

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Cleanup complete for season {SeasonId}", seasonId);
        }
        private async Task CleanupFixturesAndMatchesAsync(int seasonId)
        {
            _logger.LogInformation("🧩 Cleaning Fixtures & Matches for season {SeasonId}", seasonId);

            var fixtures = await _context.Fixtures
                .Where(f => f.SeasonId == seasonId)
                .ToListAsync();

            if (!fixtures.Any())
            {
                _logger.LogWarning("⚠️ No fixtures found for season {SeasonId}", seasonId);
                return;
            }

            var fixtureIds = fixtures.Select(f => f.Id).ToList();
            var matches = await _context.Matches
                .Where(m => fixtureIds.Contains(m.FixtureId))
                .Include(m => m.PlayerStats)
                .Include(m => m.Events)
                .Include(m => m.Penalties)
                .ToListAsync();

            _logger.LogInformation("📊 Found {MatchCount} matches and {FixtureCount} fixtures to delete",
                matches.Count, fixtures.Count);

            var playerStatsCount = matches.Sum(m => m.PlayerStats.Count);
            var eventsCount = matches.Sum(m => m.Events.Count);
            var penaltiesCount = matches.Sum(m => m.Penalties.Count);

            _context.PlayerMatchStats.RemoveRange(matches.SelectMany(m => m.PlayerStats));
            _context.MatchEvents.RemoveRange(matches.SelectMany(m => m.Events));
            _context.Penalties.RemoveRange(matches.SelectMany(m => m.Penalties));

            _logger.LogInformation("🧾 Deleted {StatsCount} player stats, {EventsCount} events, {PenaltiesCount} penalties",
                playerStatsCount, eventsCount, penaltiesCount);

            _context.Matches.RemoveRange(matches);
            _context.Fixtures.RemoveRange(fixtures);

            _logger.LogInformation("✅ Deleted all fixtures & matches for season {SeasonId}", seasonId);
        }
        private async Task CleanupTransfersAsync(int seasonId)
        {
            _logger.LogInformation("💸 Cleaning transfers for season {SeasonId}", seasonId);

            // Изтриваме оферти първо, защото имат FK към Player и Transfer
            var transferOffers = await _context.TransferOffers
                .Where(o => o.GameSaveId == seasonId)
                .ToListAsync();

            if (transferOffers.Any())
            {
                _context.TransferOffers.RemoveRange(transferOffers);
                _logger.LogInformation("✅ Deleted {OfferCount} transfer offers for season {SeasonId}", transferOffers.Count, seasonId);
            }
            else
            {
                _logger.LogInformation("⚠️ No transfer offers found for season {SeasonId}", seasonId);
            }

            // Изтриваме трансфери за сезона
            var transfers = await _context.Transfers
                .Where(t => t.SeasonId == seasonId)
                .ToListAsync();

            if (transfers.Any())
            {
                _context.Transfers.RemoveRange(transfers);
                _logger.LogInformation("✅ Deleted {TransferCount} transfers for season {SeasonId}", transfers.Count, seasonId);
            }
            else
            {
                _logger.LogInformation("⚠️ No transfers found for season {SeasonId}", seasonId);
            }

            await _context.SaveChangesAsync();
        }
        private async Task CleanupFreeAgentsAsync(int seasonId)
        {
            _logger.LogInformation("🧹 Cleaning up free agents for season {SeasonId}", seasonId);

            var playersToDelete = await _context.Players
                .Where(p => p.TeamId == null && p.GameSaveId == seasonId)
                .ToListAsync();

            if (!playersToDelete.Any())
            {
                _logger.LogInformation("⚠️ No free agents found for season {SeasonId}", seasonId);
                return;
            }

            var playerIds = playersToDelete.Select(p => p.Id).ToList();

            // Изтриваме атрибути
            _context.PlayerAttributes.RemoveRange(
                _context.PlayerAttributes
                    .Where(a => a.PlayerId.HasValue && playerIds.Contains(a.PlayerId.Value))
            );

            // Изтриваме статистики
            _context.PlayerMatchStats.RemoveRange(
                _context.PlayerMatchStats.Where(s => playerIds.Contains(s.PlayerId))
            );

            _context.PlayerSeasonStats.RemoveRange(
                _context.PlayerSeasonStats.Where(s => playerIds.Contains(s.PlayerId))
            );

            _context.PlayerCompetitionStats.RemoveRange(
                _context.PlayerCompetitionStats.Where(c => playerIds.Contains(c.PlayerId))
            );

            // Изтриваме събития в мачове
            _context.MatchEvents.RemoveRange(
                _context.MatchEvents.Where(e => playerIds.Contains(e.PlayerId))
            );

            // ⚠️ Вече не трием TransferOffers тук, защото ги трием горе в CleanupTransfersAsync

            // Изтриваме самите играчи
            _context.Players.RemoveRange(playersToDelete);

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Deleted {PlayerCount} free agent players for season {SeasonId}", playersToDelete.Count, seasonId);
        }

        private async Task CleanupPlayerMatchStatsAsync(int seasonId)
        {
            var statsToDelete = await _context.PlayerMatchStats
                .Where(s => s.SeasonId == seasonId)
                .ToListAsync();

            if (statsToDelete.Any())
            {
                _context.PlayerMatchStats.RemoveRange(statsToDelete);
                await _context.SaveChangesAsync();
            }
        }        
        private async Task CleanupTrainingSessionsAsync(int seasonId)
        {
            // Намираме всички training sessions за сезона
            var sessionsToDelete = await _context.TrainingSessions
                .Where(t => t.SeasonId == seasonId || t.GameSaveId == seasonId)
                .ToListAsync();

            if (!sessionsToDelete.Any())
                return;

            var sessionIds = sessionsToDelete.Select(s => s.Id).ToList();

            // Изтриваме всички PlayerTrainings, свързани с тях
            _context.PlayerTrainings.RemoveRange(
                _context.PlayerTrainings.Where(pt => pt.TrainingSessionId != null && sessionIds.Contains(pt.TrainingSessionId.Value))
            );

            // Изтриваме самите сесии
            _context.TrainingSessions.RemoveRange(sessionsToDelete);

            await _context.SaveChangesAsync();
        }


    }
}
