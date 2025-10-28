namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Staff;
    using static TheDugout.Data.Seed.SeedDtos;
    public static class SeedMessageTemplates
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var msgTemplatesPath = Path.Combine(seedDir, "messageTemplates.json");
            var msgTemplates = await SeedData.ReadJsonAsync<List<MessageTemplateDto>>(msgTemplatesPath);

            foreach (var t in msgTemplates)
            {
                var categoryParsed = Enum.TryParse<MessageCategory>(t.Category, out var category)
                    ? category
                    : MessageCategory.General;

                var senderParsed = Enum.TryParse<MessageSenderType>(t.Sender, out var sender)
                    ? sender
                    : MessageSenderType.System;

                var existing = await db.MessageTemplates.FirstOrDefaultAsync(x =>
                    x.Category == categoryParsed &&
                    x.SubjectTemplate == t.SubjectTemplate &&
                    x.BodyTemplate == t.BodyTemplate);

                if (existing == null)
                {
                    db.MessageTemplates.Add(new MessageTemplate
                    {
                        Category = categoryParsed,
                        SubjectTemplate = t.SubjectTemplate,
                        BodyTemplate = t.BodyTemplate
                    });
                }
                else
                {

                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} messageTemplates.", msgTemplates.Count);
        }
    }
}
