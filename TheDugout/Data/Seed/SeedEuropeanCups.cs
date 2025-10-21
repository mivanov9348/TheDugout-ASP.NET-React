namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;

    public static class SeedEuropeanCups
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var path = Path.Combine(seedDir, "europeanCup.json");
            if (!File.Exists(path))
            {
                logger.LogWarning("❌ Missing seed file: {Path}", path);
                return;
            }

            var europeanCups = await SeedData.ReadJsonAsync<List<EuropeanCupTemplate>>(path);

            // 🔍 Зареждаме вече съществуващите купи с фазите им
            var dbCups = await db.EuropeanCupTemplates
                .Include(x => x.PhaseTemplates)
                .ToListAsync();

            foreach (var ec in europeanCups)
            {
                var existing = dbCups.FirstOrDefault(x => x.Name.ToLower() == ec.Name.ToLower());

                if (existing == null)
                {
                    // 🆕 Няма я — създаваме нова с фазите
                    var newCup = new EuropeanCupTemplate
                    {
                        Name = ec.Name,
                        TeamsCount = ec.TeamsCount,
                        LeaguePhaseMatchesPerTeam = ec.LeaguePhaseMatchesPerTeam,
                        Ranking = ec.Ranking,
                        IsActive = ec.IsActive,
                        PhaseTemplates = ec.PhaseTemplates.Select(p => new EuropeanCupPhaseTemplate
                        {
                            Name = p.Name,
                            Order = p.Order,
                            IsKnockout = p.IsKnockout,
                            IsTwoLegged = p.IsTwoLegged
                        }).ToList()
                    };

                    db.EuropeanCupTemplates.Add(newCup);
                    logger.LogInformation("➕ Added new European Cup: {Cup}", ec.Name);
                }
                else
                {
                    // 🔄 Има съществуваща купа → обновяваме основните полета
                    existing.TeamsCount = ec.TeamsCount;
                    existing.LeaguePhaseMatchesPerTeam = ec.LeaguePhaseMatchesPerTeam;
                    existing.Ranking = ec.Ranking;
                    existing.IsActive = ec.IsActive;

                    // 🧩 Обновяване/добавяне на фазите
                    foreach (var phaseFromJson in ec.PhaseTemplates)
                    {
                        var existingPhase = existing.PhaseTemplates
                            .FirstOrDefault(p => p.Name.ToLower() == phaseFromJson.Name.ToLower());

                        if (existingPhase == null)
                        {
                            // ➕ Добавяме нова фаза
                            existing.PhaseTemplates.Add(new EuropeanCupPhaseTemplate
                            {
                                Name = phaseFromJson.Name,
                                Order = phaseFromJson.Order,
                                IsKnockout = phaseFromJson.IsKnockout,
                                IsTwoLegged = phaseFromJson.IsTwoLegged
                            });
                            logger.LogInformation("➕ Added phase {Phase} to {Cup}", phaseFromJson.Name, ec.Name);
                        }
                        else
                        {
                            // 📝 Обновяваме съществуващата
                            existingPhase.Order = phaseFromJson.Order;
                            existingPhase.IsKnockout = phaseFromJson.IsKnockout;
                            existingPhase.IsTwoLegged = phaseFromJson.IsTwoLegged;
                        }
                    }

                    // ❌ Премахваме фази, които вече не са в JSON-а (ако искаш синхронизация)
                    var jsonPhaseNames = ec.PhaseTemplates.Select(p => p.Name.ToLower()).ToHashSet();
                    var toRemove = existing.PhaseTemplates
                        .Where(p => !jsonPhaseNames.Contains(p.Name.ToLower()))
                        .ToList();

                    if (toRemove.Any())
                    {
                        db.EuropeanCupPhaseTemplates.RemoveRange(toRemove);
                        logger.LogInformation("🗑 Removed {Count} outdated phases from {Cup}", toRemove.Count, ec.Name);
                    }
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("✅ Seeded {Count} European Cups and their phases.", europeanCups.Count);
        }
    }
}
