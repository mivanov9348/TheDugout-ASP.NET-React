namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Common;
    public static class SeedMoneyPrizes
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "moneyprizes.json");
            if (!File.Exists(path)) return;

            var prizes = await SeedData.ReadJsonAsync<List<MoneyPrize>>(path);

            foreach (var mp in prizes)
            {
                var existing = await db.MoneyPrizes.FirstOrDefaultAsync(x => x.Code == mp.Code);
                if (existing == null)
                    db.MoneyPrizes.Add(mp);
                else
                {
                    existing.Name = mp.Name;
                    existing.Amount = mp.Amount;
                    existing.Description = mp.Description;
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} money prizes.", prizes.Count);
        }
    }
}
