namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Common;

    public static class SeedRegions
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "regions.json");
            if (!File.Exists(path)) return;

            var regions = await SeedData.ReadJsonAsync<List<Region>>(path);

            foreach (var r in regions)
            {
                var existing = await db.Regions.FirstOrDefaultAsync(x => x.Name == r.Name);
                if (existing == null)
                    db.Regions.Add(r);
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} regions.", regions.Count);
        }
    }
}
