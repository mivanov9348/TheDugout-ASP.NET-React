using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Linq;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Services.Fixture;

namespace TheDugout.Services.Cup
{
    public class CupService : ICupService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly ICupFixturesService _cupFixturesService;
        private readonly Random _random = new Random();

        public CupService(DugoutDbContext context, ICupFixturesService cupFixturesService, IFixturesHelperService fixturesHelperService)
        {
            _context = context;
            _cupFixturesService = cupFixturesService;
            _fixturesHelperService = fixturesHelperService;
        }
        public async Task InitializeCupsForGameSaveAsync(GameSave gameSave, int seasonId)
        {
            var cupTemplates = await _context.CupTemplates
                .Where(ct => ct.IsActive)
                .ToListAsync();

            var allCups = new List<Models.Competitions.Cup>();

            foreach (var template in cupTemplates)
            {
                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code == template.CountryCode);
                if (country == null)
                {
                    Console.WriteLine($"[WARN] Country not found for template {template.Name} ({template.CountryCode})");
                    continue;
                }

                var teams = gameSave.Teams
                    .Where(t => t.CountryId == country.Id)
                    .ToList();

                if (teams.Count < 2)
                {
                    Console.WriteLine($"[WARN] Not enough teams for cup {template.Name}. Found: {teams.Count}");
                    continue;
                }

                int teamsCount = teams.Count;
                int nextPowerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log2(teamsCount)));
                int roundsCount = (int)Math.Ceiling(Math.Log2(nextPowerOfTwo));

                var cup = new Models.Competitions.Cup
                {
                    TemplateId = template.Id,
                    GameSaveId = gameSave.Id,
                    SeasonId = seasonId,
                    CountryId = country.Id,
                    TeamsCount = teamsCount,
                    RoundsCount = roundsCount,
                    IsActive = true
                };

                foreach (var team in teams)
                    cup.Teams.Add(new CupTeam { TeamId = team.Id });

                allCups.Add(cup);
                _context.Cups.Add(cup);
            }

            await _context.SaveChangesAsync();

            // Генериране на fixtures след като всички купи са създадени
            if (allCups.Any())
            {
                await _cupFixturesService.GenerateAllCupFixturesAsync(seasonId, gameSave.Id, allCups);
            }
        }
    }
}
