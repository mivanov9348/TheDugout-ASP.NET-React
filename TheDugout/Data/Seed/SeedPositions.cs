namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Players;
    public static class SeedPositions
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "positions.json");
            if (!File.Exists(path)) return;

            var positions = await SeedData.ReadJsonAsync<List<Position>>(path);

            foreach (var p in positions)
            {
                var existing = await db.Positions.FirstOrDefaultAsync(x => x.Code == p.Code);
                if (existing == null)
                    db.Positions.Add(p);
                else
                    existing.Name = p.Name;
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} positions.", positions.Count);
        }
    }
}
