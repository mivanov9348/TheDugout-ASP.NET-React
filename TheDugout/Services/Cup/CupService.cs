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
        private readonly ICupFixturesService _cupFixturesService;
        private readonly Random _random;
        private readonly ILogger<CupService> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CupService>();

        public CupService(DugoutDbContext context, ICupFixturesService cupFixturesService)
        {
            _context = context;
            _cupFixturesService = cupFixturesService;
            _random = new Random();
        }
        public async Task InitializeCupsForGameSaveAsync(GameSave gameSave, int seasonId)
        {
            _logger.LogInformation("=== InitializeCupsForGameSaveAsync START for GameSaveId: {GameSaveId}, SeasonId: {SeasonId} ===", gameSave.Id, seasonId);

            var cupTemplates = await _context.CupTemplates
                .Where(ct => ct.IsActive)
                .ToListAsync();

            var allCups = new List<Cup>();

            foreach (var template in cupTemplates)
            {
                var country = await _context.Countries.FirstOrDefaultAsync(c => c.Code == template.CountryCode);
                if (country == null) continue;

                var teams = gameSave.Teams.Where(t => t.CountryId == country.Id).ToList();
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

                var cup = new Cup
                {
                    Competition = competition,
                    CompetitionId = competition.Id,
                    TemplateId = template.Id,
                    GameSaveId = gameSave.Id,
                    SeasonId = seasonId,
                    CountryId = country.Id,
                    TeamsCount = teamsCount,
                    RoundsCount = roundsCount,
                    IsActive = true
                };

                _context.Cups.Add(cup);
                await _context.SaveChangesAsync(); // Save Cup first to get Cup.Id

                var cupTeams = teams.Select(t => new CupTeam
                {
                    CupId = cup.Id,
                    TeamId = t.Id,
                    GameSaveId = gameSave.Id
                }).ToList();

                _context.CupTeams.AddRange(cupTeams);

                allCups.Add(cup);
            }

            await _context.SaveChangesAsync();

            // 🧠 Ето тук е ключът — презареждаме cups с включени Teams
            allCups = await _context.Cups
                .Include(c => c.Teams)
                .Where(c => c.GameSaveId == gameSave.Id && c.SeasonId == seasonId)
                .ToListAsync();

            if (allCups.Any())
            {
                await _cupFixturesService.GenerateInitialFixturesAsync(seasonId, gameSave.Id, allCups);
            }

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
                cup.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return cup.IsFinished;
        }
    }
}
