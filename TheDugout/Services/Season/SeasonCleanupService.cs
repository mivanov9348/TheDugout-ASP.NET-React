namespace TheDugout.Services.Season
{
    using EFCore.BulkExtensions;
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
            if (seasonId <= 0)
            {
                _logger.LogWarning("⚠️ No valid seasonId provided. Trying to find last inactive season...");

                var lastInactiveSeason = await _context.Seasons
                    .Where(s => !s.IsActive)
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (lastInactiveSeason == null)
                {
                    _logger.LogError("❌ No inactive season found to clean up!");
                    throw new Exception("No inactive season found to clean up.");
                }

                seasonId = lastInactiveSeason.Id;
                _logger.LogInformation("📅 Using last inactive season {SeasonId} for cleanup.", seasonId);
            }

            _logger.LogInformation("🧹 Starting cleanup for season {SeasonId}", seasonId);

            try
            {
                await CleanupFixturesAndMatchesAsync(seasonId);
                await CleanupTrainingSessionsAsync(seasonId);
                await CleanupTransfersAsync(seasonId);
                await CleanupFreeAgentsAsync(seasonId);

                _logger.LogInformation("✅ Cleanup complete for season {SeasonId}", seasonId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [CleanupOldSeasonDataAsync] Failed to clean season {SeasonId}.", seasonId);
                throw;
            }
        }


        // ==========================================================
        // 🧩 FIXTURES & MATCHES
        // ==========================================================
        private async Task CleanupFixturesAndMatchesAsync(int seasonId)
        {
            _logger.LogInformation("🧩 Cleaning Fixtures & Matches for season {SeasonId}", seasonId);

            var fixtureIds = await _context.Fixtures
                .Where(f => f.SeasonId == seasonId)
                .Select(f => f.Id)
                .ToListAsync();

            if (!fixtureIds.Any())
            {
                _logger.LogInformation("⚠️ No fixtures found for season {SeasonId}", seasonId);
                return;
            }

            var matchIds = await _context.Matches
                .Where(m => fixtureIds.Contains(m.FixtureId))
                .Select(m => m.Id)
                .ToListAsync();

            _logger.LogInformation("📊 Found {FixtureCount} fixtures and {MatchCount} matches to delete",
                fixtureIds.Count, matchIds.Count);

            if (matchIds.Any())
            {
                // 🔹 Delete related data (fast direct deletes)
                await _context.PlayerMatchStats
                    .Where(p => matchIds.Contains(p.MatchId))
                    .ExecuteDeleteAsync();

                await _context.MatchEvents
                    .Where(e => matchIds.Contains(e.MatchId))
                    .ExecuteDeleteAsync();

                await _context.Penalties
                    .Where(p => matchIds.Contains(p.MatchId ?? -1))
                    .ExecuteDeleteAsync();

                _logger.LogInformation("🗑️ Deleted PlayerMatchStats, Events, and Penalties");

                await _context.Matches
                    .Where(m => matchIds.Contains(m.Id))
                    .ExecuteDeleteAsync();

                _logger.LogInformation("🧾 Deleted {Count} Matches", matchIds.Count);
            }

            await _context.Fixtures
                .Where(f => fixtureIds.Contains(f.Id))
                .ExecuteDeleteAsync();

            _logger.LogInformation("✅ Deleted {Count} Fixtures", fixtureIds.Count);
            _logger.LogInformation("🎯 Cleanup for Fixtures & Matches completed successfully.");
        }

        // ==========================================================
        // 💸 TRANSFERS
        // ==========================================================
        private async Task CleanupTransfersAsync(int seasonId)
        {
            _logger.LogInformation("💸 Cleaning transfers for season {SeasonId}", seasonId);

            // Изтриваме оферти (първо, защото имат FK)
            var offersDeleted = await _context.TransferOffers
                .Where(o => o.GameSave.CurrentSeasonId == seasonId)
                .ExecuteDeleteAsync();

            _logger.LogInformation("✅ Deleted {Count} transfer offers for season {SeasonId}", offersDeleted, seasonId);

            var transfersDeleted = await _context.Transfers
                .Where(t => t.SeasonId == seasonId)
                .ExecuteDeleteAsync();

            _logger.LogInformation("✅ Deleted {Count} transfers for season {SeasonId}", transfersDeleted, seasonId);
        }

        // ==========================================================
        // 🧑‍🎓 TRAINING SESSIONS
        // ==========================================================
        private async Task CleanupTrainingSessionsAsync(int seasonId)
        {
            _logger.LogInformation("💪 Cleaning training sessions for season {SeasonId}", seasonId);

            // 🔹 Първо изтрий PlayerTrainings с подзаявка
            var playerTrainingsDeleted = await _context.Database.ExecuteSqlRawAsync($@"
        DELETE FROM PlayerTrainings
        WHERE TrainingSessionId IN (
            SELECT Id FROM TrainingSessions
            WHERE SeasonId = {seasonId} OR GameSaveId IN (
                SELECT GameSaveId FROM Seasons WHERE Id = {seasonId}
            )
        )
    ");

            _logger.LogInformation("🧹 Deleted {Count} PlayerTrainings for season {SeasonId}", playerTrainingsDeleted, seasonId);

            // 🔹 После изтрий самите TrainingSessions
            var trainingSessionsDeleted = await _context.Database.ExecuteSqlRawAsync($@"
        DELETE FROM TrainingSessions
        WHERE SeasonId = {seasonId} OR GameSaveId IN (
            SELECT GameSaveId FROM Seasons WHERE Id = {seasonId}
        )
    ");

            _logger.LogInformation("✅ Deleted {Count} TrainingSessions for season {SeasonId}", trainingSessionsDeleted, seasonId);
        }


        // ==========================================================
        // 🧍‍♂️ FREE AGENTS
        // ==========================================================
        private async Task CleanupFreeAgentsAsync(int seasonId)
        {
            _logger.LogInformation("🧹 Cleaning up free agents for season {SeasonId}", seasonId);

            var playerIds = await _context.Players
                .Where(p => p.TeamId == null)
                .Select(p => p.Id)
                .ToListAsync();

            if (!playerIds.Any())
            {
                _logger.LogInformation("⚠️ No free agents found for season {SeasonId}", seasonId);
                return;
            }

            await _context.PlayerAttributes
                .Where(a => a.PlayerId.HasValue && playerIds.Contains(a.PlayerId.Value))
                .ExecuteDeleteAsync();

            await _context.PlayerMatchStats
                .Where(s => playerIds.Contains(s.PlayerId))
                .ExecuteDeleteAsync();

            await _context.PlayerSeasonStats
                .Where(s => playerIds.Contains(s.PlayerId))
                .ExecuteDeleteAsync();

            await _context.PlayerCompetitionStats
                .Where(c => playerIds.Contains(c.PlayerId))
                .ExecuteDeleteAsync();

            await _context.MatchEvents
                .Where(e => playerIds.Contains(e.PlayerId))
                .ExecuteDeleteAsync();

            await _context.Players
                .Where(p => playerIds.Contains(p.Id))
                .ExecuteDeleteAsync();

            _logger.LogInformation("✅ Deleted {Count} free agent players and related data", playerIds.Count);
        }
    }
}
