namespace TheDugout.Data.Seed
{
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Messages;

    public static class SeedMessages
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "messages.json");
            if (!File.Exists(path)) return;

            var messages = await SeedData.ReadJsonAsync<List<MessageTemplate>>(path);

            if (!db.MessageTemplates.Any())
            {
                db.MessageTemplates.AddRange(messages);
                await db.SaveChangesAsync();
            }

            logger.LogInformation("Seeded {Count} message templates.", messages.Count);
        }
    }
}
