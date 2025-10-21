namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Common;

    public static class SeedCountries
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "countries.json");
            if (!File.Exists(path)) return;

            var countries = await SeedData.ReadJsonAsync<List<Country>>(path);

            foreach (var c in countries)
            {
                var existing = await db.Countries.FirstOrDefaultAsync(x => x.Name == c.Name);
                if (existing == null)
                    db.Countries.Add(c);
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} countries.", countries.Count);
        }
    }
}
