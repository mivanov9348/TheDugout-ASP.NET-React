namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using TheDugout.Models.Players;
    using static TheDugout.Data.Seed.SeedDtos;

    public static class SeedAttributes
    {
        public static async Task<Dictionary<string, AttributeDefinition>> RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var attributesPath = Path.Combine(seedDir, "attributes.json");
            var attributes = await SeedData.ReadJsonAsync<List<AttributeDefinition>>(attributesPath);

            foreach (var a in attributes)
            {
                var existing = await db.Attributes.FirstOrDefaultAsync(x => x.Code == a.Code);
                if (existing == null)
                {
                    db.Attributes.Add(new Models.Players.AttributeDefinition
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

            // Position Weights
            var weightsPath = Path.Combine(seedDir, "positionWeights.json");
            var weights = await SeedData.ReadJsonAsync<List<PositionWeightDto>>(weightsPath);
            var positionsByCode = await db.Positions.ToDictionaryAsync(x => x.Code, x => x);

            foreach (var w in weights)
            {
                if (!positionsByCode.TryGetValue(w.PositionCode, out var position))
                {
                    logger.LogWarning("Weight references missing position {PositionCode}", w.PositionCode);
                    continue;
                }
                if (!attributesByCode.TryGetValue(w.AttributeCode, out var attribute))
                {
                    logger.LogWarning("Weight references missing attribute {AttributeCode}", w.AttributeCode);
                    continue;
                }

                var existing = await db.PositionWeights
                    .FirstOrDefaultAsync(x => x.PositionId == position.Id && x.AttributeId == attribute.Id);

                if (existing == null)
                {
                    db.PositionWeights.Add(new PositionWeight
                    {
                        PositionId = position.Id,
                        AttributeId = attribute.Id,
                        Weight = w.Weight
                    });
                }
                else
                {
                    existing.Weight = w.Weight;
                }
            }
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded or updated {Count} attributes.", attributes.Count);
            return attributesByCode;
        }
    }
}
