namespace TheDugout.Services.Season
{
    using Microsoft.EntityFrameworkCore;
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
            await CleanupTransfersAndFreeAgentsAsync(seasonId);

            // clean transfers
            // clean freeagents
            // clean player match stats
            // training sesisons

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Cleanup complete for season {SeasonId}", seasonId);
        }
        private async Task CleanupFixturesAndMatchesAsync(int seasonId)
        {
            _logger.LogInformation("🧩 Cleaning Fixtures & Matches for season {SeasonId}", seasonId);

            // 1️⃣ Намираме всички фикстури за сезона
            var fixtures = await _context.Fixtures
                .Where(f => f.SeasonId == seasonId)
                .ToListAsync();

            if (!fixtures.Any())
            {
                _logger.LogWarning("⚠️ No fixtures found for season {SeasonId}", seasonId);
                return;
            }

            // 2️⃣ Извличаме всички мачове, свързани с тези фикстури
            var fixtureIds = fixtures.Select(f => f.Id).ToList();
            var matches = await _context.Matches
                .Where(m => fixtureIds.Contains(m.FixtureId))
                .Include(m => m.PlayerStats)
                .Include(m => m.Events)
                .Include(m => m.Penalties)
                .ToListAsync();

            _logger.LogInformation("📊 Found {MatchCount} matches and {FixtureCount} fixtures to delete",
                matches.Count, fixtures.Count);

            // 3️⃣ Изтриваме зависимите обекти първо
            var playerStatsCount = matches.Sum(m => m.PlayerStats.Count);
            var eventsCount = matches.Sum(m => m.Events.Count);
            var penaltiesCount = matches.Sum(m => m.Penalties.Count);

            _context.PlayerMatchStats.RemoveRange(matches.SelectMany(m => m.PlayerStats));
            _context.MatchEvents.RemoveRange(matches.SelectMany(m => m.Events));
            _context.Penalties.RemoveRange(matches.SelectMany(m => m.Penalties));

            _logger.LogInformation("🧾 Deleted {StatsCount} player stats, {EventsCount} events, {PenaltiesCount} penalties",
                playerStatsCount, eventsCount, penaltiesCount);

            // 4️⃣ Изтриваме мачовете и фикстурите
            _context.Matches.RemoveRange(matches);
            _context.Fixtures.RemoveRange(fixtures);

            _logger.LogInformation("✅ Deleted all fixtures & matches for season {SeasonId}", seasonId);
        }
        private async Task CleanupTransfersAndFreeAgentsAsync(int seasonId)
        {
            _logger.LogInformation("💸 Cleaning Transfers & Free Agents for season {SeasonId}", seasonId);

            // 1️⃣ Изтриваме трансфери за сезона
            var transfers = await _context.Transfers
                .Where(t => t.SeasonId == seasonId)
                .ToListAsync();

            if (!transfers.Any())
            {
                _logger.LogWarning("⚠️ No transfers found for season {SeasonId}", seasonId);
            }
            else
            {
                _context.Transfers.RemoveRange(transfers);
                _logger.LogInformation("✅ Deleted {TransferCount} transfers for season {SeasonId}", transfers.Count, seasonId);
            }

            // 2️⃣ (По-късно ще добавим FreeAgents ако имаш таблица)
            _logger.LogInformation("⚙️ Free agents cleanup skipped — not implemented yet.");
        }
    }
}
