namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Common;

    public static class SeedRegions
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var regionsPath = Path.Combine(seedDir, "regions.json");
            var regions = await SeedData.ReadJsonAsync<List<Region>>(regionsPath);

            foreach (var r in regions)
            {
                var existing = await db.Regions.FirstOrDefaultAsync(x => x.Code == r.Code);
                if (existing == null)
                {
                    db.Regions.Add(new Region { Code = r.Code, Name = r.Name });
                }
                else if (existing.Name != r.Name)
                {
                    existing.Name = r.Name;
                }
            }
            await db.SaveChangesAsync();

            var regionsByCode = await db.Regions.ToDictionaryAsync(x => x.Code, x => x);
            logger.LogInformation("Seeded {Count} regions.", regions.Count);
        }
    }
}
