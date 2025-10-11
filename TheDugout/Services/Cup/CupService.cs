namespace TheDugout.Services.Cup
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Xml.Linq;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Services.Fixture;

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

            var allCups = new List<Models.Cups.Cup>();

            foreach (var template in cupTemplates)
            {
                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code == template.CountryCode);

                if (country == null) continue;

                var teams = gameSave.Teams
                    .Where(t => t.CountryId == country.Id)
                    .ToList();

                if (teams.Count < 2) continue;

                int teamsCount = teams.Count;
                int nextPowerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log2(teamsCount)));
                int roundsCount = (int)Math.Ceiling(Math.Log2(nextPowerOfTwo));

                var competition = new Competition
                {
                    Type = CompetitionTypeEnum.DomesticCup,
                    GameSaveId = gameSave.Id,
                    SeasonId = seasonId
                };

                // Shared PK pattern — Cup.Id == Competition.Id
                var cup = new Models.Cups.Cup
                {
                    Competition = competition,
                    TemplateId = template.Id,
                    GameSaveId = gameSave.Id,
                    SeasonId = seasonId,
                    CountryId = country.Id,
                    TeamsCount = teamsCount,
                    RoundsCount = roundsCount,
                    IsActive = true
                };

                _context.Cups.Add(cup);
                await _context.SaveChangesAsync();

                // Добавяме участници
                foreach (var team in teams)
                    cup.Teams.Add(new CupTeam { TeamId = team.Id, GameSaveId = gameSave.Id });

                allCups.Add(cup);
            }

            if (allCups.Any())
                await _cupFixturesService.GenerateInitialFixturesAsync(seasonId, gameSave.Id, allCups);

            await _context.SaveChangesAsync();
        }


    }
}
