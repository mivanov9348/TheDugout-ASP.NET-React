namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Teams;

    public static class SeedTactics
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "tactics.json");
            if (!File.Exists(path)) return;

            var tactics = await SeedData.ReadJsonAsync<List<Tactic>>(path);

            foreach (var t in tactics)
            {
                var existing = await db.Tactics.FirstOrDefaultAsync(x => x.Name == t.Name);
                if (existing == null)
                    db.Tactics.Add(t);
                else
                {
                    existing.Defenders = t.Defenders;
                    existing.Midfielders = t.Midfielders;
                    existing.Forwards = t.Forwards;
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} tactics.", tactics.Count);
        }
    }
}
