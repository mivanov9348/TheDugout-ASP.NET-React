namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Teams;
    public static class SeedTeams
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "teams.json");
            if (!File.Exists(path)) return;

            var teams = await SeedData.ReadJsonAsync<List<Team>>(path);

            foreach (var t in teams)
            {
                var existing = await db.Teams.FirstOrDefaultAsync(x => x.Name == t.Name);
                if (existing == null)
                    db.Teams.Add(t);
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} teams.", teams.Count);
        }
    }
}
