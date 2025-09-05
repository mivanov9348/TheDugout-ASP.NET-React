using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TheDugout.Data;
using TheDugout.Models;
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

        // 0) Positions
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

        // 1) Attributes
        var attributesPath = Path.Combine(seedDir, "attributes.json");
        var attributes = await ReadJsonAsync<List<Models.Attribute>>(attributesPath);

        foreach (var a in attributes)
        {
            var existing = await db.Attributes.FirstOrDefaultAsync(x => x.Code == a.Code);
            if (existing == null)
            {
                db.Attributes.Add(new Models.Attribute
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

        // 2) Position Weights
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

        // 3) Countries
        var countriesPath = Path.Combine(seedDir, "countries.json");
        var countries = await ReadJsonAsync<List<CountryDto>>(countriesPath);

        foreach (var c in countries)
        {
            var existing = await db.Countries.FirstOrDefaultAsync(x => x.Code == c.Code);
            if (existing == null)
            {
                db.Countries.Add(new Country { Code = c.Code, Name = c.Name });
            }
            else if (existing.Name != c.Name)
            {
                existing.Name = c.Name;
            }
        }
        await db.SaveChangesAsync();

        var countriesByCode = await db.Countries.ToDictionaryAsync(x => x.Code, x => x);

        // 4) Leagues
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

        // 5) Teams
        var teamsDir = Path.Combine(seedDir, "teams");

        // ще пазим всички тимове за валидирането после
        var allTeams = new List<TeamTemplateDto>();

        foreach (var file in Directory.GetFiles(teamsDir, "*.json"))
        {
            var teams = await ReadJsonAsync<List<TeamTemplateDto>>(file);
            allTeams.AddRange(teams);

            foreach (var t in teams)
            {
                if (!leaguesByCode.TryGetValue(t.CompetitionCode, out var league))
                {
                    logger.LogWarning("Team {Team} references missing league {LeagueCode}", t.Name, t.CompetitionCode);
                    continue;
                }

                // Ключ: (Abbreviation, CountryId) – предполагаме уникални в рамките на държава
                var existing = await db.TeamTemplates
                    .FirstOrDefaultAsync(x => x.Abbreviation == t.ShortName && x.CountryId == league.CountryId);

                if (existing == null)
                {
                    db.TeamTemplates.Add(new TeamTemplate
                    {
                        Name = t.Name,
                        Abbreviation = t.ShortName,
                        CountryId = league.CountryId,
                        LeagueId = league.Id
                    });
                }
                else
                {
                    existing.Name = t.Name;
                    existing.CountryId = league.CountryId;
                    existing.LeagueId = league.Id;
                }
            }
        }

        await db.SaveChangesAsync();


        // 6) MessageTemplates
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


        // 4) Валидирай брой отбори спрямо лигите (информативно)
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
