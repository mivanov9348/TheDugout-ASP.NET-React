namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Teams;

    public static class SeedCommentaryTemplates
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var commentaryFile = Path.Combine(seedDir, "commentaryTemplates.json");
            var commentaryTemplates = await SeedData.ReadJsonAsync<List<SeedDtos.CommentaryTemplateDto>>(commentaryFile);

            var dbOutcomesByKey = await db.EventOutcomes
                .ToDictionaryAsync(x => (x.EventTypeCode, x.Name));

            var dbCommentary = await db.CommentaryTemplates.ToListAsync();

            foreach (var ct in commentaryTemplates)
            {
                if (!dbOutcomesByKey.TryGetValue((ct.EventTypeCode, ct.OutcomeName), out var outcome))
                {
                    logger.LogWarning("Unknown outcome '{Outcome}' for EventType '{Code}'", ct.OutcomeName, ct.EventTypeCode);
                    continue;
                }

                var existing = dbCommentary.FirstOrDefault(x =>
                    x.EventOutcomeId == outcome.Id &&
                    x.Template == ct.Template);

                if (existing == null)
                {
                    db.CommentaryTemplates.Add(new CommentaryTemplate
                    {
                        EventTypeCode = ct.EventTypeCode,
                        OutcomeName = ct.OutcomeName,
                        Template = ct.Template,
                        EventOutcomeId = outcome.Id
                    });
                }
                else
                {
                    if (existing.Template != ct.Template)
                    {
                        existing.Template = ct.Template;
                        db.CommentaryTemplates.Update(existing);
                    }
                }
            }
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} teams.", commentaryTemplates.Count);

        }
    }
}
