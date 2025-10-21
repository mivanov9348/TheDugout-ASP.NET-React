namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Cups;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
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
        }
    }
       
}
