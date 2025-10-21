namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using TheDugout.Data;
    using TheDugout.Data.Seed;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Players;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;
    using static TheDugout.Data.Seed.SeedDtos;

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

            // Tactics
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

            // Positions
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

            // Money Prizes
            var moneyPrizesPath = Path.Combine(seedDir, "moneyprizes.json");
            var moneyPrizes = await ReadJsonAsync<List<MoneyPrize>>(moneyPrizesPath);

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

            // Attributes
            var attributesPath = Path.Combine(seedDir, "attributes.json");
            var attributes = await ReadJsonAsync<List<Models.Players.AttributeDefinition>>(attributesPath);

            foreach (var a in attributes)
            {
                var existing = await db.Attributes.FirstOrDefaultAsync(x => x.Code == a.Code);
                if (existing == null)
                {
                    db.Attributes.Add(new Models.Players.AttributeDefinition
                    {
                        Code = a.Code,
                        Name = a.Name,
                        Category = a.Category
                    });
                }
                else
                {
                    if (existing.Name != a.Name)
                        existing.Name = a.Name;

                    if (existing.Category != a.Category)
                        existing.Category = a.Category;
                }
            }
            await db.SaveChangesAsync();

            var attributesByCode = await db.Attributes.ToDictionaryAsync(x => x.Code, x => x);

            // Position Weights
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

            // Regions
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

            // Settings
            var settingsPath = Path.Combine(seedDir, "settings.json");

            if (!File.Exists(settingsPath))
                return;

            var json = await File.ReadAllTextAsync(settingsPath);
            var settings = JsonSerializer.Deserialize<List<GameSetting>>(json);

            if (settings == null || settings.Count == 0)
                return;

            foreach (var s in settings)
            {
                var existing = await db.Set<GameSetting>().FirstOrDefaultAsync(x => x.Key == s.Key);

                if (existing == null)
                {
                    db.Set<GameSetting>().Add(new GameSetting
                    {
                        Key = s.Key,
                        Value = s.Value
                    });
                }
                else if (existing.Value != s.Value)
                {
                    // Ако стойността е променена в JSON → ъпдейт
                    existing.Value = s.Value;
                }
            }

            await db.SaveChangesAsync();
            // Countries
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

            // First Names
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

            // Last Names
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

            // Leagues
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
                }
            }
            await db.SaveChangesAsync();

            var leaguesByCode = await db.LeagueTemplates
                .Include(x => x.Country)
                .ToDictionaryAsync(x => x.LeagueCode, x => x);

            // Cups
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
            // EventTypes
            var eventTypesFile = Path.Combine(seedDir, "eventTypes.json");
            var eventTypes = await ReadJsonAsync<List<SeedDtos.EventTypeDto>>(eventTypesFile);
            var dbEventTypes = await db.EventTypes.ToListAsync();

            foreach (var et in eventTypes)
            {
                var existing = dbEventTypes.FirstOrDefault(x => x.Code == et.Code);
                if (existing == null)
                {
                    db.EventTypes.Add(new EventType
                    {
                        Code = et.Code,
                        Name = et.Name
                    });
                }
                else
                {
                    if (existing.Name != et.Name)
                    {
                        existing.Name = et.Name;
                        db.EventTypes.Update(existing);
                    }
                }
            }
            await db.SaveChangesAsync();

            // EventAttributeWeights
            var eventWeightsFile = Path.Combine(seedDir, "matchEventWeights.json");
            var eventWeights = await ReadJsonAsync<List<SeedDtos.EventAttributeWeightDto>>(eventWeightsFile);

            var dbEventTypesWithAttrs = await db.EventTypes
                .Include(et => et.AttributeWeights)
                .ToListAsync();

            var dbAttributes = await db.Attributes.ToListAsync();

            foreach (var ew in eventWeights)
            {
                var eventType = dbEventTypesWithAttrs.FirstOrDefault(x => x.Code == ew.EventTypeCode);
                if (eventType == null) continue;

                foreach (var attr in ew.Attributes)
                {
                    var existing = eventType.AttributeWeights.FirstOrDefault(x => x.AttributeCode == attr.AttributeCode);
                    if (existing == null)
                    {
                        var attribute = dbAttributes.FirstOrDefault(a => a.Code == attr.AttributeCode);
                        if (attribute == null) continue;

                        db.EventAttributeWeights.Add(new EventAttributeWeight
                        {
                            EventTypeCode = ew.EventTypeCode,
                            EventType = eventType,
                            AttributeCode = attr.AttributeCode,
                            Attribute = attribute,
                            Weight = attr.Weight
                        });
                    }
                    else
                    {
                        if (Math.Abs(existing.Weight - attr.Weight) > 0.0001)
                        {
                            existing.Weight = attr.Weight;
                            db.EventAttributeWeights.Update(existing);
                        }
                    }
                }
            }

            await db.SaveChangesAsync();

            // EventOutcomes
            var outcomesFile = Path.Combine(seedDir, "eventOutcomes.json");
            var eventOutcomes = await ReadJsonAsync<List<SeedDtos.EventOutcomeDto>>(outcomesFile);
            var dbOutcomes = await db.EventOutcomes.Include(o => o.EventType).ToListAsync();
            var typesByCode = await db.EventTypes.ToDictionaryAsync(x => x.Code);

            foreach (var eo in eventOutcomes)
            {
                if (!typesByCode.TryGetValue(eo.EventTypeCode, out var type))
                {
                    logger.LogWarning("Unknown EventTypeCode '{EventTypeCode}' for outcome '{Name}'", eo.EventTypeCode, eo.Name);
                    continue;
                }

                // Търсим по EventTypeId и Range (уникалната комбинация за outcome), вместо само по име
                var existing = dbOutcomes.FirstOrDefault(x => x.EventTypeId == type.Id && x.Name == eo.Name);

                if (existing == null)
                {
                    // Ако няма → създаваме
                    db.EventOutcomes.Add(new EventOutcome
                    {
                        Name = eo.Name,
                        EventTypeId = type.Id,
                        EventTypeCode = eo.EventTypeCode,
                        ChangesPossession = eo.ChangesPossession,
                        RangeMin = eo.RangeMin,
                        RangeMax = eo.RangeMax
                    });
                }
                else
                {
                    // Ако има → презаписваме всички полета (не само част от тях)
                    existing.Name = eo.Name;
                    existing.EventTypeId = type.Id;
                    existing.EventTypeCode = eo.EventTypeCode;
                    existing.ChangesPossession = eo.ChangesPossession;
                    existing.RangeMin = eo.RangeMin;
                    existing.RangeMax = eo.RangeMax;

                    db.EventOutcomes.Update(existing);
                }
            }

            await db.SaveChangesAsync();

            // CommentaryTemplates
            var commentaryFile = Path.Combine(seedDir, "commentaryTemplates.json");
            var commentaryTemplates = await ReadJsonAsync<List<SeedDtos.CommentaryTemplateDto>>(commentaryFile);

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

            // Teams
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

            // --- European Cups and Phases Seeding ---
            var europeanCupsPath = Path.Combine(seedDir, "europeanCup.json");
            var europeanCups = await ReadJsonAsync<List<EuropeanCupTemplate>>(europeanCupsPath);

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

        public static async Task<T> ReadJsonAsync<T>(string path)
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
}

//namespace TheDugout.Data.Seed
//{
//    using Microsoft.EntityFrameworkCore;
//    using Microsoft.Extensions.DependencyInjection;
//    using Microsoft.Extensions.Hosting;
//    using Microsoft.Extensions.Logging;
//    using System.Text.Json;
//    public static class SeedData
//    {
//        public static async Task EnsureSeededAsync(IServiceProvider services, ILogger logger)
//        {
//            using var scope = services.CreateScope();
//            var db = scope.ServiceProvider.GetRequiredService<DugoutDbContext>();
//            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
//            var seedDir = Path.Combine(env.ContentRootPath, "Data", "SeedFiles");

//            await db.Database.MigrateAsync();

//            await SeedTactics.RunAsync(db, seedDir, logger);
//            await SeedPositions.RunAsync(db, seedDir, logger);
//            await SeedMoneyPrizes.RunAsync(db, seedDir, logger);
//            await SeedAttributes.RunAsync(db, seedDir, logger);
//            await SeedRegions.RunAsync(db, seedDir, logger);
//            await SeedCountries.RunAsync(db, seedDir, logger);
//            await SeedNames.RunAsync(db, seedDir, logger);
//            await SeedLeaguesAndCups.RunAsync(db, seedDir, logger);
//            await SeedTeams.RunAsync(db, seedDir, logger);
//            await SeedMessages.RunAsync(db, seedDir, logger);
//            await SeedEuropeanCups.RunAsync(db, seedDir, logger);

//            logger.LogInformation("✅ All seed data applied successfully.");
//        }

//        public static async Task<T> ReadJsonAsync<T>(string path)
//        {
//            if (!File.Exists(path))
//                throw new FileNotFoundException($"Seed file not found: {path}");

//            using var fs = File.OpenRead(path);
//            return (await JsonSerializer.DeserializeAsync<T>(fs, new JsonSerializerOptions
//            {
//                PropertyNameCaseInsensitive = true,
//                ReadCommentHandling = JsonCommentHandling.Skip,
//                AllowTrailingCommas = true
//            }))!;
//        }
//    }
//}
