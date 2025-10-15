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
    using TheDugout.Services.Cup.Interfaces;
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
                .Where(f => f.Status != FixtureStatusEnum.Cancelled)
                .All(f => f.Status == FixtureStatusEnum.Played);

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
        public async Task<List<CompetitionSeasonResult>> GenerateCupResultsAsync(int seasonId)
        {
            var cups = await _context.Cups
                .Include(c => c.Country)
                .Include(c => c.Template)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                .Where(c => c.SeasonId == seasonId && c.IsFinished)
                .ToListAsync();

            var results = new List<CompetitionSeasonResult>();

            foreach (var cup in cups)
            {
                // Намираме финалния рунд
                var finalRound = cup.Rounds
                    .OrderByDescending(r => r.RoundNumber)
                    .FirstOrDefault();

                if (finalRound == null)
                    continue;

                // Взимаме финалния мач
                var finalMatch = finalRound.Fixtures
                    .Where(f => f.Status == FixtureStatusEnum.Played)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                if (finalMatch == null)
                    continue;

                // Определяме шампиона и финалиста чрез WinnerTeamId
                int? championTeamId = finalMatch.WinnerTeamId;
                int? runnerUpTeamId = null;

                if (championTeamId == finalMatch.HomeTeamId)
                    runnerUpTeamId = finalMatch.AwayTeamId;
                else if (championTeamId == finalMatch.AwayTeamId)
                    runnerUpTeamId = finalMatch.HomeTeamId;

                // fallback ако WinnerTeamId липсва
                if (championTeamId == null)
                {
                    if (finalMatch.HomeTeamGoals > finalMatch.AwayTeamGoals)
                    {
                        championTeamId = finalMatch.HomeTeamId;
                        runnerUpTeamId = finalMatch.AwayTeamId;
                    }
                    else
                    {
                        championTeamId = finalMatch.AwayTeamId;
                        runnerUpTeamId = finalMatch.HomeTeamId;
                    }
                }

                // Създаваме CompetitionSeasonResult
                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.DomesticCup,
                    CompetitionId = cup.CompetitionId,
                    GameSaveId = cup.GameSaveId,
                    ChampionTeamId = championTeamId,
                    RunnerUpTeamId = runnerUpTeamId,
                    Notes = $"Купа {cup.Template.Name} ({cup.Country.Name}) - Финал: {finalMatch.HomeTeam?.Name} {finalMatch.HomeTeamGoals}:{finalMatch.AwayTeamGoals} {finalMatch.AwayTeam?.Name}"
                };

                // Купата носи квота за Европа
                if (championTeamId.HasValue)
                {
                    result.EuropeanQualifiedTeams.Add(new CompetitionEuropeanQualifiedTeam
                    {
                        TeamId = championTeamId.Value,
                        GameSaveId = cup.GameSaveId
                    });
                }

                results.Add(result);
            }

            if (results.Any())
            {
                await _context.CompetitionSeasonResults.AddRangeAsync(results);
                await _context.SaveChangesAsync();
            }

            return results;
        }
    }
}
