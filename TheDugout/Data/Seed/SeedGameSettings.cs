namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;
    using TheDugout.Models.Common;
    public static class SeedGameSettings
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var settingsPath = Path.Combine(seedDir, "settings.json");

            if (!File.Exists(settingsPath))
                return;

            var json = await File.ReadAllTextAsync(settingsPath);
            var settings = JsonSerializer.Deserialize<List<GameSetting>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settings == null || settings.Count == 0)
                return;

            foreach (var s in settings)
            {
                var existing = await db.Set<GameSetting>().FirstOrDefaultAsync(x => x.Key == s.Key);

                if (existing == null)
                {
                    // 🆕 Нов запис
                    db.Set<GameSetting>().Add(new GameSetting
                    {
                        Key = s.Key,
                        Value = s.Value,
                        Category = s.Category,
                        Description = s.Description
                    });
                }
                else
                {
                    // 🔁 Проверка за промени и ъпдейт
                    bool updated = false;

                    if (existing.Value != s.Value)
                    {
                        existing.Value = s.Value;
                        updated = true;
                    }

                    if (existing.Category != s.Category)
                    {
                        existing.Category = s.Category;
                        updated = true;
                    }

                    if (existing.Description != s.Description)
                    {
                        existing.Description = s.Description;
                        updated = true;
                    }

                    if (updated)
                        db.Set<GameSetting>().Update(existing);
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} money prizes.", settings.Count);
        }
    }
}
