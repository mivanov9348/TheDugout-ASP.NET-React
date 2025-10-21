namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using TheDugout.Models.Common;

    public static class SeedNames
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            // --- Подготвяме lookup на регионите ---
            var regionsByCode = await db.Regions.ToDictionaryAsync(r => r.Code);

            // === FIRST NAMES ===
            var firstNamesPath = Path.Combine(seedDir, "firstnames.json");
            if (File.Exists(firstNamesPath))
            {
                var firstNamesDict = await SeedData.ReadJsonAsync<Dictionary<string, List<string>>>(firstNamesPath);

                foreach (var kvp in firstNamesDict)
                {
                    var regionCode = kvp.Key;
                    if (!regionsByCode.TryGetValue(regionCode, out var region))
                    {
                        logger.LogWarning("FirstNames references missing region {RegionCode}", regionCode);
                        continue;
                    }

                    foreach (var name in kvp.Value)
                    {
                        var exists = await db.FirstNames
                            .FirstOrDefaultAsync(x => x.Name == name && x.RegionCode == regionCode);

                        if (exists == null)
                        {
                            db.FirstNames.Add(new FirstName
                            {
                                Name = name,
                                RegionCode = regionCode,
                                Region = region
                            });
                        }
                    }
                }

                await db.SaveChangesAsync();
                logger.LogInformation("Seeded first names from {Path}", firstNamesPath);
            }
            else
            {
                logger.LogWarning("Missing file: {Path}", firstNamesPath);
            }

            // === LAST NAMES ===
            var lastNamesPath = Path.Combine(seedDir, "lastnames.json");
            if (File.Exists(lastNamesPath))
            {
                var lastNamesDict = await SeedData.ReadJsonAsync<Dictionary<string, List<string>>>(lastNamesPath);

                foreach (var kvp in lastNamesDict)
                {
                    var regionCode = kvp.Key;
                    if (!regionsByCode.TryGetValue(regionCode, out var region))
                    {
                        logger.LogWarning("LastNames references missing region {RegionCode}", regionCode);
                        continue;
                    }

                    foreach (var name in kvp.Value)
                    {
                        var exists = await db.LastNames
                            .FirstOrDefaultAsync(x => x.Name == name && x.RegionCode == regionCode);

                        if (exists == null)
                        {
                            db.LastNames.Add(new LastName
                            {
                                Name = name,
                                RegionCode = regionCode,
                                Region = region
                            });
                        }
                    }
                }

                await db.SaveChangesAsync();
                logger.LogInformation("Seeded last names from {Path}", lastNamesPath);
            }
            else
            {
                logger.LogWarning("Missing file: {Path}", lastNamesPath);
            }
        }
    }
}
