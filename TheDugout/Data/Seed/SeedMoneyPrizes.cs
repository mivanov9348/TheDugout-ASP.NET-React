namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Common;
    public static class SeedMoneyPrizes
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var moneyPrizesPath = Path.Combine(seedDir, "moneyprizes.json");
            var moneyPrizes = await SeedData.ReadJsonAsync<List<MoneyPrize>>(moneyPrizesPath);

            foreach (var mp in moneyPrizes)
            {
                var existing = await db.MoneyPrizes.FirstOrDefaultAsync(x => x.Code == mp.Code);
                if (existing == null)
                {
                    db.MoneyPrizes.Add(new MoneyPrize
                    {
                        Code = mp.Code,
                        Name = mp.Name,
                        Amount = mp.Amount,
                        Description = mp.Description,
                        IsActive = mp.IsActive
                    });
                }
                else
                {
                    bool updated = false;

                    if (existing.Name != mp.Name) { existing.Name = mp.Name; updated = true; }
                    if (existing.Amount != mp.Amount) { existing.Amount = mp.Amount; updated = true; }
                    if (existing.Description != mp.Description) { existing.Description = mp.Description; updated = true; }
                    if (existing.IsActive != mp.IsActive) { existing.IsActive = mp.IsActive; updated = true; }

                    if (updated)
                        db.MoneyPrizes.Update(existing);
                }
            }

            await db.SaveChangesAsync();

            var moneyPrizesByCode = await db.MoneyPrizes.ToDictionaryAsync(x => x.Code, x => x);
            logger.LogInformation("Seeded {Count} money prizes.", moneyPrizes.Count);
        }
    }
}
