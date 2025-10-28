namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Teams;
    using static TheDugout.Data.Seed.SeedDtos;

    public static class SeedLeaguesAndCups
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            // --- Подготвяме lookup за страните ---
            var countriesByCode = await db.Countries.ToDictionaryAsync(c => c.Code);

            // === LEAGUES ===
            var leaguesPath = Path.Combine(seedDir, "leagues.json");
            if (File.Exists(leaguesPath))
            {
                var leagues = await SeedData.ReadJsonAsync<List<LeagueTemplateDto>>(leaguesPath);

                foreach (var l in leagues)
                {
                    if (!countriesByCode.TryGetValue(l.CountryCode, out var country))
                    {
                        logger.LogWarning("League {LeagueCode} references missing country {CountryCode}", l.Code, l.CountryCode);
                        continue;
                    }

                    var existing = await db.LeagueTemplates
                        .FirstOrDefaultAsync(x => x.LeagueCode == l.Code);

                    if (existing == null)
                    {
                        db.LeagueTemplates.Add(new LeagueTemplate
                        {
                            LeagueCode = l.Code,
                            Name = l.Name,
                            CountryId = country.Id,
                            Tier = l.Tier,
                            TeamsCount = l.Teams,
                            RelegationSpots = l.RelegationSpots,
                            PromotionSpots = l.PromotionSpots,
                            IsActive = l.IsActive
                        });
                    }
                    else
                    {
                        existing.Name = l.Name;
                        existing.CountryId = country.Id;
                        existing.Tier = l.Tier;
                        existing.TeamsCount = l.Teams;
                        existing.RelegationSpots = l.RelegationSpots;
                        existing.PromotionSpots = l.PromotionSpots;
                        existing.IsActive = l.IsActive;
                    }
                }

                await db.SaveChangesAsync();
                logger.LogInformation("Seeded/Updated {Count} league templates.", leagues.Count);
            }
            else
            {
                logger.LogWarning("Missing file: {Path}", leaguesPath);
            }

            // --- Зареждаме отново лигите, ако ще ни трябват за връзки ---
            var leaguesByCode = await db.LeagueTemplates
                .Include(x => x.Country)
                .ToDictionaryAsync(x => x.LeagueCode, x => x);

            // === CUPS ===
            var cupsPath = Path.Combine(seedDir, "cups.json");
            if (File.Exists(cupsPath))
            {
                var cups = await SeedData.ReadJsonAsync<List<CupTemplateDto>>(cupsPath);

                foreach (var c in cups)
                {
                    var existing = await db.CupTemplates
                        .FirstOrDefaultAsync(x => x.Name == c.Name && x.CountryCode == c.CountryCode);

                    if (existing == null)
                    {
                        db.CupTemplates.Add(new CupTemplate
                        {
                            Name = c.Name,
                            CountryCode = c.CountryCode,
                            IsActive = c.IsActive,
                            MinTeams = c.MinTeams,
                            MaxTeams = c.MaxTeams
                        });
                    }
                    else
                    {
                        existing.IsActive = c.IsActive;
                        existing.MinTeams = c.MinTeams;
                        existing.MaxTeams = c.MaxTeams;
                    }
                }

                await db.SaveChangesAsync();
                logger.LogInformation("Seeded/Updated {Count} cup templates.", cups.Count);
            }
            else
            {
                logger.LogWarning("Missing file: {Path}", cupsPath);
            }

            // Teams
            var teamsDir = Path.Combine(seedDir, "teams");
            var allTeams = new List<TeamTemplateDto>();


            foreach (var file in Directory.GetFiles(teamsDir, "*.json"))
            {
                var teams = await SeedData.ReadJsonAsync<List<TeamTemplateDto>>(file);
                allTeams.AddRange(teams);
            }

            var dbTeams = await db.TeamTemplates.ToListAsync();

            foreach (var t in allTeams)
            {
                int? countryId = null;
                string? countryCode = null;

                if (string.IsNullOrEmpty(t.CompetitionCode))
                {
                    var existing = dbTeams
                        .FirstOrDefault(x => x.Abbreviation == t.ShortName && x.LeagueId == null);

                    if (!string.IsNullOrWhiteSpace(t.CountryCode))
                    {
                        countryCode = t.CountryCode.Trim().ToUpper();
                        if (countriesByCode.TryGetValue(countryCode, out var country))
                        {
                            countryId = country.Id;
                        }
                        else
                        {
                            logger.LogWarning("Team '{Team}' has invalid or unknown country code: {CountryCode}", t.Name, t.CountryCode);
                        }
                    }

                    if (existing == null)
                    {
                        db.TeamTemplates.Add(new TeamTemplate
                        {
                            Name = t.Name,
                            Abbreviation = t.ShortName,
                            CountryId = countryId,
                            LeagueId = null,
                            CountryCode = countryCode // Записваме само ако е зададен
                        });
                    }
                    else
                    {
                        bool needsUpdate = false;

                        if (existing.Name != t.Name)
                        {
                            existing.Name = t.Name;
                            needsUpdate = true;
                        }

                        if (existing.CountryId != countryId)
                        {
                            existing.CountryId = countryId;
                            needsUpdate = true;
                        }

                        if (existing.CountryCode != countryCode)
                        {
                            existing.CountryCode = countryCode;
                            needsUpdate = true;
                        }

                        if (needsUpdate)
                        {
                            db.TeamTemplates.Update(existing);
                        }
                    }

                    continue;
                }

                // Нормален отбор с лига
                if (!leaguesByCode.TryGetValue(t.CompetitionCode, out var league))
                {
                    logger.LogWarning("Team {Team} references missing league {LeagueCode}", t.Name, t.CompetitionCode);
                    continue;
                }

                var existingLeagueTeam = dbTeams
                    .FirstOrDefault(x => x.Abbreviation == t.ShortName && x.LeagueId == league.Id);

                // 👉 ЛОГИКА ЗА ОТБОРИ С ЛИГА — ПРИОРИТЕТ НА country-code ОТ JSON
                if (!string.IsNullOrWhiteSpace(t.CountryCode))
                {
                    // Ако има валиден countryCode в JSON — използвай го
                    countryCode = t.CountryCode.Trim().ToUpper();
                    if (countriesByCode.TryGetValue(countryCode, out var country))
                    {
                        countryId = country.Id;
                    }
                    else
                    {
                        logger.LogWarning("Team '{Team}' has invalid or unknown country code: {CountryCode} (league: {League})",
                            t.Name, t.CountryCode, t.CompetitionCode);
                        // Не падаме на лигата — просто оставяме countryId = null и ще паднем по-долу
                    }
                }

                if (countryId == null)
                {
                    countryId = league.CountryId;
                }

                // --- СЪЗДАВАНЕ / ОБНОВЯВАНЕ НА ОТБОРА ---
                if (existingLeagueTeam == null)
                {
                    db.TeamTemplates.Add(new TeamTemplate
                    {
                        Name = t.Name,
                        Abbreviation = t.ShortName,
                        CountryId = countryId,
                        LeagueId = league.Id,
                        CountryCode = countryCode
                    });
                }
                else
                {
                    bool needsUpdate = false;

                    if (existingLeagueTeam.Name != t.Name)
                    {
                        existingLeagueTeam.Name = t.Name;
                        needsUpdate = true;
                    }

                    if (existingLeagueTeam.LeagueId != league.Id)
                    {
                        existingLeagueTeam.LeagueId = league.Id;
                        needsUpdate = true;
                    }

                    if (existingLeagueTeam.CountryId != countryId)
                    {
                        existingLeagueTeam.CountryId = countryId;
                        needsUpdate = true;
                    }

                    // 👉 ПРОМЯНА: само ако имаме явно зададен countryCode — го запазваме
                    // Ако беше "NED", а сега е null (от лигата) — го зануляваме!
                    if (existingLeagueTeam.CountryCode != countryCode)
                    {
                        existingLeagueTeam.CountryCode = countryCode;
                        needsUpdate = true;
                    }

                    if (needsUpdate)
                    {
                        db.TeamTemplates.Update(existingLeagueTeam);
                    }
                }
            }

            var jsonKeys = allTeams
                .Select(t => (t.ShortName, string.IsNullOrEmpty(t.CompetitionCode) ? null : t.CompetitionCode))
                .ToHashSet();

            var toRemove = dbTeams
                .Where(x => !jsonKeys.Contains((
                    x.Abbreviation,
                    x.LeagueId == null ? null : x.League.LeagueCode
                )))
                .ToList();

            if (toRemove.Any())
            {
                db.TeamTemplates.RemoveRange(toRemove);
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} teams.", allTeams.Count);
        }
    }
}


