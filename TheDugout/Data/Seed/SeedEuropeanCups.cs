namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Competitions;
    using static TheDugout.Data.Seed.SeedDtos;

    public static class SeedEuropeanCups
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var europeanCupsPath = Path.Combine(seedDir, "europeanCup.json");
            var europeanCups = await SeedData.ReadJsonAsync<List<EuropeanCupTemplate>>(europeanCupsPath);

            var teamsDir = Path.Combine(seedDir, "teams");
            var allTeams = new List<TeamTemplateDto>();

            foreach (var file in Directory.GetFiles(teamsDir, "*.json"))
            {
                var teams = await SeedData.ReadJsonAsync<List<TeamTemplateDto>>(file);
                allTeams.AddRange(teams);
            }

            var leaguesPath = Path.Combine(seedDir, "leagues.json");
            var leagues = await SeedData.ReadJsonAsync<List<LeagueTemplateDto>>(leaguesPath);

            // 1) Подготвяме lookup за вече съществуващи купи и фази
            var dbCups = await db.EuropeanCupTemplates
                .Include(x => x.PhaseTemplates)
                .ToListAsync();

            foreach (var ec in europeanCups)
            {
                // Намираме купата по име (Case-insensitive just in case)
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
                }
                else
                {
                    // 🔄 Има съществуваща купа → обновяваме основните полета
                    existing.TeamsCount = ec.TeamsCount;
                    existing.LeaguePhaseMatchesPerTeam = ec.LeaguePhaseMatchesPerTeam;
                    existing.Ranking = ec.Ranking;
                    existing.IsActive = ec.IsActive;

                    // 🧩 Обновяване на фазите (без да трием излишно)
                    foreach (var phaseFromJson in ec.PhaseTemplates)
                    {
                        var existingPhase = existing.PhaseTemplates
                            .FirstOrDefault(p => p.Name.ToLower() == phaseFromJson.Name.ToLower());

                        if (existingPhase == null)
                        {
                            // ➕ Добавяме само ако няма такава
                            existing.PhaseTemplates.Add(new EuropeanCupPhaseTemplate
                            {
                                Name = phaseFromJson.Name,
                                Order = phaseFromJson.Order,
                                IsKnockout = phaseFromJson.IsKnockout,
                                IsTwoLegged = phaseFromJson.IsTwoLegged
                            });
                        }
                        else
                        {
                            // 📝 Обновяваме съществуващата
                            existingPhase.Order = phaseFromJson.Order;
                            existingPhase.IsKnockout = phaseFromJson.IsKnockout;
                            existingPhase.IsTwoLegged = phaseFromJson.IsTwoLegged;
                        }
                    }

                    // ❌ Премахваме фази, които ги няма вече в JSON-а (ако искаш да се чистят)
                    var jsonPhaseNames = ec.PhaseTemplates.Select(p => p.Name.ToLower()).ToHashSet();
                    var toRemoveExisted = existing.PhaseTemplates
                        .Where(p => !jsonPhaseNames.Contains(p.Name.ToLower()))
                        .ToList();

                    if (toRemoveExisted.Any())
                        db.EuropeanCupPhaseTemplates.RemoveRange(toRemoveExisted);
                }
            }

            await db.SaveChangesAsync();

            // Валидирай брой отбори спрямо лигите
            var teamsByLeague = allTeams
                .GroupBy(x => x.CompetitionCode)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var l in leagues)
            {
                if (teamsByLeague.TryGetValue(l.Code, out var count) && count != l.Teams)
                {
                    logger.LogWarning("League {League} expects {Expected} teams, but teams.json has {Actual}",
                        l.Code, l.Teams, count);
                }
            }
        }
    }
}

