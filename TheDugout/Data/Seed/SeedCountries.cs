namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Common;
    using static TheDugout.Data.Seed.SeedDtos;
    public static class SeedCountries
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var countriesPath = Path.Combine(seedDir, "countries.json");
            var countries = await SeedData.ReadJsonAsync<List<CountryDto>>(countriesPath);

            foreach (var c in countries)
            {
                var existing = await db.Countries.FirstOrDefaultAsync(x => x.Code == c.Code);
                if (existing == null)
                {
                    db.Countries.Add(new Models.Common.Country
                    {
                        Code = c.Code,
                        Name = c.Name,
                        RegionCode = c.RegionCode
                    });
                }
                else
                {
                    existing.Name = c.Name;

                    if (existing.RegionCode != c.RegionCode)
                        existing.RegionCode = c.RegionCode;
                }
            }
            await db.SaveChangesAsync();

            var countriesByCode = await db.Countries.ToDictionaryAsync(x => x.Code, x => x);
            logger.LogInformation("Seeded {Count} countries.", countries.Count);
        }
    }
}
