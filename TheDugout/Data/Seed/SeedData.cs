namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;

    public static class SeedData
    {
        public static async Task EnsureSeededAsync(IServiceProvider services, ILogger logger)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DugoutDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var seedDir = Path.Combine(env.ContentRootPath, "Data", "SeedFiles");

            await db.Database.MigrateAsync();

            await SeedTactics.RunAsync(db, seedDir, logger);
            await SeedPositions.RunAsync(db, seedDir, logger);
            await SeedMoneyPrizes.RunAsync(db, seedDir, logger);
            await SeedAttributes.RunAsync(db, seedDir, logger);
            await SeedRegions.RunAsync(db, seedDir, logger);
            await SeedCountries.RunAsync(db, seedDir, logger);
            await SeedNames.RunAsync(db, seedDir, logger);
            await SeedLeaguesAndCups.RunAsync(db, seedDir, logger);
            await SeedMessages.RunAsync(db, seedDir, logger);
            await SeedEuropeanCups.RunAsync(db, seedDir, logger);
            await SeedAgencies.RunAsync(db, seedDir, logger);
            await SeedCommentaryTemplates.RunAsync(db, seedDir, logger);
            await SeedEvents.RunAsync(db, seedDir, logger);
            await SeedGameSettings.RunAsync(db, seedDir, logger);
            await SeedMessageTemplates.RunAsync(db, seedDir, logger);
            

            logger.LogInformation("✅ All seed data applied successfully.");
        }

        public static async Task<T> ReadJsonAsync<T>(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Seed file not found: {path}");

            using var fs = File.OpenRead(path);
            return (await JsonSerializer.DeserializeAsync<T>(fs, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            }))!;
        }
    }
}
