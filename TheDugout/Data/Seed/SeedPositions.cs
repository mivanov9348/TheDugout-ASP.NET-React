namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Players;
    public static class SeedPositions
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var positionsPath = Path.Combine(seedDir, "positions.json");
            var positions = await SeedData.ReadJsonAsync<List<Position>>(positionsPath);

            foreach (var p in positions)
            {
                var existing = await db.Positions.FirstOrDefaultAsync(x => x.Code == p.Code);
                if (existing == null)
                {
                    db.Positions.Add(new Position
                    {
                        Code = p.Code,
                        Name = p.Name
                    });
                }
                else if (existing.Name != p.Name)
                {
                    existing.Name = p.Name;
                }
            }
            await db.SaveChangesAsync();

            var positionsByCode = await db.Positions.ToDictionaryAsync(x => x.Code, x => x);
            logger.LogInformation("Seeded {Count} positions.", positions.Count);
        }
    }
}
