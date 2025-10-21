namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Players;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public static class SeedAttributes
    {
        public static async Task<Dictionary<string, AttributeDefinition>> RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var attributesPath = Path.Combine(seedDir, "attributes.json");
            if (!File.Exists(attributesPath))
            {
                logger.LogWarning("Missing file: {Path}", attributesPath);
                return new Dictionary<string, AttributeDefinition>();
            }

            var attributes = await SeedData.ReadJsonAsync<List<AttributeDefinition>>(attributesPath);

            foreach (var a in attributes)
            {
                var existing = await db.Attributes.FirstOrDefaultAsync(x => x.Code == a.Code);
                if (existing == null)
                {
                    db.Attributes.Add(new AttributeDefinition
                    {
                        Code = a.Code,
                        Name = a.Name,
                        Category = a.Category
                    });
                }
                else
                {
                    if (existing.Name != a.Name)
                        existing.Name = a.Name;

                    if (existing.Category != a.Category)
                        existing.Category = a.Category;
                }
            }

            await db.SaveChangesAsync();

            var attributesByCode = await db.Attributes.ToDictionaryAsync(x => x.Code, x => x);

            logger.LogInformation("Seeded or updated {Count} attributes.", attributes.Count);
            return attributesByCode;
        }
    }
}
