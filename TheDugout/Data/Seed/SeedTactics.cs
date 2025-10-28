namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Teams;
    using static TheDugout.Data.Seed.SeedDtos;

    public static class SeedTactics
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var tacticsPath = Path.Combine(seedDir, "tactics.json");
            var tactics = await SeedData.ReadJsonAsync<List<TacticDto>>(tacticsPath);

            foreach (var t in tactics)
            {
                var existing = await db.Tactics.FirstOrDefaultAsync(x => x.Name == t.Name);
                if (existing == null)
                {
                    db.Tactics.Add(new Tactic
                    {
                        Name = t.Name,
                        Defenders = t.Defenders,
                        Midfielders = t.Midfielders,
                        Forwards = t.Forwards,
                        TeamTactics = new List<TeamTactic>()
                    });
                }
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
