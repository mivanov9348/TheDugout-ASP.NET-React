using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TheDugout.Data;
using TheDugout.Models.Common;
using TheDugout.Models.Competitions;
using TheDugout.Models.Messages;
using TheDugout.Models.Players;
using TheDugout.Models.Staff;
using TheDugout.Models.Teams;
using static TheDugout.Data.Seed.SeedDtos;

namespace TheDugout.Infrastructure;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DugoutDbContext>();

        // Миграции при старт
        await db.Database.MigrateAsync();

        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var seedDir = Path.Combine(env.ContentRootPath, "Data", "SeedFiles");

        // 1) Tactics
        var tacticsPath = Path.Combine(seedDir, "tactics.json");
        var tactics = await ReadJsonAsync<List<TacticDto>>(tacticsPath);

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

        // 2) Positions
        var positionsPath = Path.Combine(seedDir, "positions.json");
        var positions = await ReadJsonAsync<List<Position>>(positionsPath);

        foreach (var p in positions)
        {
            var existing = await db.Positions.FirstOrDefaultAsync(x => x.Code == p.Code);
            if (existing == null)
            {
                db.Positions.Add(new Position
                {
                    Code = p.Code,
                    Name = p.Name
                });
            }
            else if (existing.Name != p.Name)
            {
                existing.Name = p.Name;
            }
        }
        await db.SaveChangesAsync();

        var positionsByCode = await db.Positions.ToDictionaryAsync(x => x.Code, x => x);

        // 3) Attributes
        var attributesPath = Path.Combine(seedDir, "attributes.json");
        var attributes = await ReadJsonAsync<List<Models.Players.Attribute>>(attributesPath);

        foreach (var a in attributes)
        {
            var existing = await db.Attributes.FirstOrDefaultAsync(x => x.Code == a.Code);
            if (existing == null)
            {
                db.Attributes.Add(new Models.Players.Attribute
                {
                    Code = a.Code,
                    Name = a.Name
                });
            }
            else if (existing.Name != a.Name)
            {
                existing.Name = a.Name;
            }
        }
        await db.SaveChangesAsync();

        var attributesByCode = await db.Attributes.ToDictionaryAsync(x => x.Code, x => x);

        // 4) Position Weights
        var weightsPath = Path.Combine(seedDir, "positionWeights.json");
        var weights = await ReadJsonAsync<List<PositionWeightDto>>(weightsPath);

        foreach (var w in weights)
        {
            if (!positionsByCode.TryGetValue(w.PositionCode, out var position))
            {
                logger.LogWarning("Weight references missing position {PositionCode}", w.PositionCode);
                continue;
            }
            if (!attributesByCode.TryGetValue(w.AttributeCode, out var attribute))
            {
                logger.LogWarning("Weight references missing attribute {AttributeCode}", w.AttributeCode);
                continue;
            }

            var existing = await db.PositionWeights
                .FirstOrDefaultAsync(x => x.PositionId == position.Id && x.AttributeId == attribute.Id);

            if (existing == null)
            {
                db.PositionWeights.Add(new PositionWeight
                {
                    PositionId = position.Id,
                    AttributeId = attribute.Id,
                    Weight = w.Weight
                });
            }
            else
            {
                existing.Weight = w.Weight;
            }
        }
        await db.SaveChangesAsync();

        // 5) Regions
        var regionsPath = Path.Combine(seedDir, "regions.json");
        var regions = await ReadJsonAsync<List<Region>>(regionsPath);

        foreach (var r in regions)
        {
            var existing = await db.Regions.FirstOrDefaultAsync(x => x.Code == r.Code);
            if (existing == null)
            {
                db.Regions.Add(new Region { Code = r.Code, Name = r.Name });
            }
            else if (existing.Name != r.Name)
            {
                existing.Name = r.Name;
            }
        }
        await db.SaveChangesAsync();

        var regionsByCode = await db.Regions.ToDictionaryAsync(x => x.Code, x => x);

        // 6) Countries
        var countriesPath = Path.Combine(seedDir, "countries.json");
        var countries = await ReadJsonAsync<List<CountryDto>>(countriesPath);

        foreach (var c in countries)
        {
            var existing = await db.Countries.FirstOrDefaultAsync(x => x.Code == c.Code);
            if (existing == null)
            {
                db.Countries.Add(new Models.Common.Country
                {
                    Code = c.Code,
                    Name = c.Name,
                    RegionCode = c.RegionCode 
                });
            }
            else
            {
                existing.Name = c.Name;

                if (existing.RegionCode != c.RegionCode)
                    existing.RegionCode = c.RegionCode;
            }
        }
        await db.SaveChangesAsync();

        var countriesByCode = await db.Countries.ToDictionaryAsync(x => x.Code, x => x);

        // 7) First Names
        var firstNamesPath = Path.Combine(seedDir, "firstnames.json");
        var firstNamesDict = await ReadJsonAsync<Dictionary<string, List<string>>>(firstNamesPath);

        foreach (var kvp in firstNamesDict)
        {
            var regionCode = kvp.Key;
            if (!regionsByCode.TryGetValue(regionCode, out var region))
            {
                logger.LogWarning("FirstNames references missing region {RegionCode}", regionCode);
                continue;
            }

            foreach (var name in kvp.Value)
            {
                var exists = await db.FirstNames.FirstOrDefaultAsync(x => x.Name == name && x.RegionCode == regionCode);
                if (exists == null)
                {
                    db.FirstNames.Add(new FirstName
                    {
                        Name = name,
                        RegionCode = regionCode,
                        Region = region
                    });
                }
            }
        }
        await db.SaveChangesAsync();

        // 8) Last Names
        var lastNamesPath = Path.Combine(seedDir, "lastnames.json");
        var lastNamesDict = await ReadJsonAsync<Dictionary<string, List<string>>>(lastNamesPath);

        foreach (var kvp in lastNamesDict)
        {
            var regionCode = kvp.Key;
            if (!regionsByCode.TryGetValue(regionCode, out var region))
            {
                logger.LogWarning("LastNames references missing region {RegionCode}", regionCode);
                continue;
            }

            foreach (var name in kvp.Value)
            {
                var exists = await db.LastNames.FirstOrDefaultAsync(x => x.Name == name && x.RegionCode == regionCode);
                if (exists == null)
                {
                    db.LastNames.Add(new LastName
                    {
                        Name = name,
                        RegionCode = regionCode,
                        Region = region
                    });
                }
            }
        }
        await db.SaveChangesAsync();

        // 9) Leagues
        var leaguesPath = Path.Combine(seedDir, "leagues.json");
        var leagues = await ReadJsonAsync<List<LeagueTemplateDto>>(leaguesPath);

        foreach (var l in leagues)
        {
            if (!countriesByCode.TryGetValue(l.CountryCode, out var country))
            {
                logger.LogWarning("League {LeagueCode} references missing country {CountryCode}", l.Code, l.CountryCode);
                continue;
            }

            var existing = await db.LeagueTemplates.FirstOrDefaultAsync(x => x.LeagueCode == l.Code);
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
                    PromotionSpots = l.PromotionSpots
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
            }
        }
        await db.SaveChangesAsync();

        var leaguesByCode = await db.LeagueTemplates
            .Include(x => x.Country)
            .ToDictionaryAsync(x => x.LeagueCode, x => x);

        // 4) Cups
        var cupsPath = Path.Combine(seedDir, "cups.json");
        var cupTemplates = await ReadJsonAsync<List<CupTemplateDto>>(cupsPath);

        foreach (var c in cupTemplates)
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

        // 10) Teams
        var teamsDir = Path.Combine(seedDir, "teams");
        var allTeams = new List<TeamTemplateDto>();

        foreach (var file in Directory.GetFiles(teamsDir, "*.json"))
        {
            var teams = await ReadJsonAsync<List<TeamTemplateDto>>(file);
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

        // Agencies
        var agenciesPath = Path.Combine(seedDir, "agencies.json");
        var agencies = await ReadJsonAsync<List<AgencyTemplateDto>>(agenciesPath);

        foreach (var agencyDto in agencies)
        {
            var existing = await db.AgencyTemplates
                .FirstOrDefaultAsync(x => x.Name == agencyDto.Name);

            if (existing == null)
            {
                var newAgency = new AgencyTemplate
                {
                    Id = agencyDto.Id,
                    Name = agencyDto.Name,
                    RegionCode = agencyDto.RegionCode,
                    IsActive = agencyDto.IsActive
                };

                db.AgencyTemplates.Add(newAgency);
            }
            else
            {
                existing.RegionCode = agencyDto.RegionCode;
                existing.IsActive = agencyDto.IsActive;
            }
        }

        await db.SaveChangesAsync();

        // MessageTemplates
        var msgTemplatesPath = Path.Combine(seedDir, "messageTemplates.json");
        var msgTemplates = await ReadJsonAsync<List<MessageTemplateDto>>(msgTemplatesPath);

        foreach (var t in msgTemplates)
        {
            var categoryParsed = Enum.TryParse<MessageCategory>(t.Category, out var category)
                ? category
                : MessageCategory.General;

            var existing = await db.MessageTemplates.FirstOrDefaultAsync(x =>
                x.Category == category &&
                x.SubjectTemplate == t.SubjectTemplate &&
                x.BodyTemplate == t.BodyTemplate);

            if (existing == null)
            {
                db.MessageTemplates.Add(new MessageTemplate
                {
                    Category = categoryParsed,
                    SubjectTemplate = t.SubjectTemplate,
                    BodyTemplate = t.BodyTemplate,
                    PlaceholdersJson = JsonSerializer.Serialize(t.Placeholders),
                    Weight = t.Weight,
                    IsActive = t.IsActive,
                    Language = t.Language
                });
            }
            else
            {
                existing.Weight = t.Weight;
                existing.IsActive = t.IsActive;
                existing.Language = t.Language;
                existing.PlaceholdersJson = JsonSerializer.Serialize(t.Placeholders);
            }
        }

        await db.SaveChangesAsync();

        // European Cup Phases
        var phasesPath = Path.Combine(seedDir, "europeanCupPhase.json");
        var phases = await ReadJsonAsync<List<EuropeanCupPhaseTemplate>>(phasesPath);

        foreach (var p in phases)
        {
            var existing = await db.EuropeanCupPhaseTemplates.FirstOrDefaultAsync(x => x.Name == p.Name);
            if (existing == null)
            {
                db.EuropeanCupPhaseTemplates.Add(new EuropeanCupPhaseTemplate
                {
                    Name = p.Name,
                    Order = p.Order,
                    IsKnockout = p.IsKnockout,
                    IsTwoLegged = p.IsTwoLegged
                });
            }
            else
            {
                existing.Order = p.Order;
                existing.IsKnockout = p.IsKnockout;
                existing.IsTwoLegged = p.IsTwoLegged;
            }
        }
        await db.SaveChangesAsync();

        var dbPhases = await db.EuropeanCupPhaseTemplates.ToDictionaryAsync(x => x.Id, x => x);

        // 13) European Cups
        var europeanCupsPath = Path.Combine(seedDir, "europeanCup.json");
        var europeanCups = await ReadJsonAsync<List<EuropeanCupTemplate>>(europeanCupsPath);

        foreach (var ec in europeanCups)
        {
            var existing = await db.EuropeanCupTemplates
                .Include(x => x.PhaseTemplates)
                .FirstOrDefaultAsync(x => x.Name == ec.Name);

            if (existing == null)
            {
                var newCup = new EuropeanCupTemplate
                {
                    Name = ec.Name,
                    TeamsCount = ec.TeamsCount,
                    LeaguePhaseMatchesPerTeam = ec.LeaguePhaseMatchesPerTeam,
                    Ranking = ec.Ranking,
                    IsActive = ec.IsActive,
                };

                if (ec.PhaseTemplates != null && ec.PhaseTemplates.Count > 0)
                {
                    foreach (var phaseId in ec.PhaseTemplates.Select(p => p.Id))
                    {
                        if (dbPhases.TryGetValue(phaseId, out var phase))
                        {
                            newCup.PhaseTemplates.Add(phase);
                        }
                    }
                }

                db.EuropeanCupTemplates.Add(newCup);
            }
            else
            {
                existing.TeamsCount = ec.TeamsCount;
                existing.LeaguePhaseMatchesPerTeam = ec.LeaguePhaseMatchesPerTeam;
                existing.Ranking = ec.Ranking;
                existing.IsActive = ec.IsActive;

                existing.PhaseTemplates.Clear();
                if (ec.PhaseTemplates != null && ec.PhaseTemplates.Count > 0)
                {
                    foreach (var phaseId in ec.PhaseTemplates.Select(p => p.Id))
                    {
                        if (dbPhases.TryGetValue(phaseId, out var phase))
                        {
                            existing.PhaseTemplates.Add(phase);
                        }
                    }
                }
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

    private static async Task<T> ReadJsonAsync<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        using var fs = File.OpenRead(path);
        return (await JsonSerializer.DeserializeAsync<T>(fs, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        }))!;
    }
}
