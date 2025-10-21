namespace TheDugout.Services.Cup
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.Fixture;
    public class CupService : ICupService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly ICupFixturesService _cupFixturesService;
        private readonly Random _random = new Random();
        private readonly ILogger<CupService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CupService>();

        public CupService(DugoutDbContext context, ICupFixturesService cupFixturesService, IFixturesHelperService fixturesHelperService)
        {
            _context = context;
            _cupFixturesService = cupFixturesService;
            _fixturesHelperService = fixturesHelperService;
        }
        public async Task InitializeCupsForGameSaveAsync(GameSave gameSave, int seasonId)
        {
            _logger.LogInformation("=== InitializeCupsForGameSaveAsync START for GameSaveId: {GameSaveId}, SeasonId: {SeasonId} ===", gameSave.Id, seasonId);

            var cupTemplates = await _context.CupTemplates
                .Where(ct => ct.IsActive)
                .ToListAsync();

            _logger.LogInformation("Loaded {Count} active CupTemplates.", cupTemplates.Count);

            var allCups = new List<Models.Cups.Cup>();

            foreach (var template in cupTemplates)
            {
                _logger.LogInformation("Processing CupTemplate {TemplateId} ({CountryCode})", template.Id, template.CountryCode);

                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code == template.CountryCode);

                if (country == null)
                {
                    _logger.LogWarning("Skipping template {TemplateId} - Country not found.", template.Id);
                    continue;
                }

                var teams = gameSave.Teams
                    .Where(t => t.CountryId == country.Id)
                    .ToList();

                _logger.LogInformation("Found {Count} teams for country {CountryId}", teams.Count, country.Id);

                if (teams.Count < 2)
                {
                    _logger.LogWarning("Skipping cup for {CountryCode} - Not enough teams ({Count})", country.Code, teams.Count);
                    continue;
                }

                int teamsCount = teams.Count;
                int nextPowerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log2(teamsCount)));
                int roundsCount = (int)Math.Ceiling(Math.Log2(nextPowerOfTwo));

                var competition = new Competition
                {
                    Type = CompetitionTypeEnum.DomesticCup,
                    GameSaveId = gameSave.Id,
                    SeasonId = seasonId
                };

                var cup = new Cup
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

                _logger.LogInformation("Created Cup {CupId} for CountryId {CountryId} with {TeamsCount} teams.", cup.Id, country.Id, teamsCount);

                foreach (var team in teams)
                {
                    var entry = _context.Entry(team);
                    _logger.LogInformation("Team {TeamId} State: {State}", team.Id, entry.State);
                    cup.Teams.Add(new CupTeam { TeamId = team.Id, GameSaveId = gameSave.Id });
                }

                allCups.Add(cup);

                await _context.SaveChangesAsync();
            }

            if (allCups.Any())
            {
                _logger.LogInformation("Generating initial fixtures for {Count} cups...", allCups.Count);
                await _cupFixturesService.GenerateInitialFixturesAsync(seasonId, gameSave.Id, allCups);
            }
            else
            {
                _logger.LogWarning("No cups created for GameSaveId {GameSaveId}.", gameSave.Id);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("=== InitializeCupsForGameSaveAsync END ===");
        }
        public async Task<bool> IsCupFinishedAsync(int cupId)
        {
            // Getting the cup with its rounds and fixtures
            var cup = await _context.Cups
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                .Include(c => c.Teams)
                .FirstOrDefaultAsync(c => c.Id == cupId);

            if (cup == null)
                throw new Exception("Cup not found");

            // Check if all rounds are finished
            bool allRoundsFinished = cup.Rounds
                .SelectMany(r => r.Fixtures)
                .All(f => f.Status == MatchStageEnum.Played);

            // Check if only one team is left (not eliminated)
            bool onlyOneTeamLeft = cup.Teams.Count(t => !t.IsEliminated) <= 1;

            // If either condition is met, mark the cup as finished
            if ((allRoundsFinished || onlyOneTeamLeft) && !cup.IsFinished)
            {
                cup.IsFinished = true;
                cup.IsActive = false; // Deactivate the cup
                await _context.SaveChangesAsync();
            }

            return cup.IsFinished;
        }
    }
}
