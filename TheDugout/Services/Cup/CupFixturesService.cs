using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Cups;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Game;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;
using TheDugout.Services.Fixture;
using TheDugout.Services.Season;

namespace TheDugout.Services.Cup
{
    public class CupFixturesService : ICupFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixtureHelperService;
        private readonly ICupScheduleService _cupScheduleService;
        private readonly Random _random = new Random();

        public CupFixturesService(
            DugoutDbContext context,
            IFixturesHelperService fixtureHelperService,
            ICupScheduleService cupScheduleService
            )
        {
            _context = context;
            _fixtureHelperService = fixtureHelperService;
            _cupScheduleService = cupScheduleService;
        }

        public async Task GenerateAllCupFixturesAsync(
       int seasonId,
       int gameSaveId,
       List<Models.Cups.Cup> cups)
        {
            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null) return;

            var allFixtures = new List<Models.Fixtures.Fixture>();

            var cupTeamsMap = new Dictionary<int, List<Models.Teams.Team>>();
            foreach (var cup in cups)
            {
                // само отборите, които не са елиминирани
                var teams = cup.Teams
                    .Where(ct => !ct.IsEliminated)
                    .Select(ct => new Models.Teams.Team
                    {
                        Id = ct.TeamId,
                        Name = ct.Team?.Name ?? $"Team {ct.TeamId}"
                    })
                    .ToList();

                if (teams.Count < 2)
                    continue;

                teams = teams.OrderBy(_ => _random.Next()).ToList();
                cupTeamsMap[cup.Id] = teams;
            }

            bool anyNeedsPrelim = cupTeamsMap.Values.Any(t => NeedsPreliminaryRound(t.Count));

            foreach (var cup in cups)
            {
                if (!cupTeamsMap.TryGetValue(cup.Id, out var teams)) continue;

                var fixturesForCup = GenerateFixturesForCup(
                    cup,
                    teams,
                    gameSaveId,
                    seasonId,
                    anyNeedsPrelim
                );

                allFixtures.AddRange(fixturesForCup);
            }

            if (allFixtures.Any())
            {
                _cupScheduleService.AssignCupFixtures(allFixtures, season);
                await _context.Fixtures.AddRangeAsync(allFixtures);
                await _context.SaveChangesAsync();
            }
        }

        private List<Models.Fixtures.Fixture> GenerateFixturesForCup(
        Models.Cups.Cup cup,
        List<Models.Teams.Team> teams,
        int gameSaveId,
        int seasonId,
        bool globalNeedsPrelim)
        {
            var fixtures = new List<Models.Fixtures.Fixture>();
            bool needsPrelim = NeedsPreliminaryRound(teams.Count);
            int prelimTeamsCount = GetTeamsInPrelimRound(teams.Count);

            if (needsPrelim && prelimTeamsCount > 0)
            {
                var prelimTeams = teams.Take(prelimTeamsCount * 2).ToList();
                var prelimRound = new CupRound
                {
                    CupId = cup.Id,
                    RoundNumber = 1,
                    Name = "Preliminary Round"
                };

                for (int i = 0; i < prelimTeamsCount; i++)
                {
                    var home = prelimTeams[i * 2];
                    var away = prelimTeams[i * 2 + 1];

                    var fixture = _fixtureHelperService.CreateFixture(
                        gameSaveId,
                        seasonId,
                        home.Id,
                        away.Id,
                        DateTime.MinValue,
                        1,
                        CompetitionType.DomesticCup,
                        prelimRound
                    );

                    fixture.CupRound = prelimRound;
                    fixtures.Add(fixture);
                }

                _context.CupRounds.Add(prelimRound);
                return fixtures;
            }

            if (globalNeedsPrelim)
            {
                return fixtures;
            }

            var teamsForFirstRound = new List<Models.Teams.Team>(teams);

            if (teamsForFirstRound.Count >= 2)
            {
                var firstRound = new CupRound
                {
                    CupId = cup.Id,
                    RoundNumber = 1,
                    Name = "Round 1"
                };

                if (teamsForFirstRound.Count % 2 != 0)
                {
                    teamsForFirstRound = teamsForFirstRound.Take(teamsForFirstRound.Count - 1).ToList();
                }

                for (int i = 0; i < teamsForFirstRound.Count; i += 2)
                {
                    var home = teamsForFirstRound[i];
                    var away = teamsForFirstRound[i + 1];

                    var fixture = _fixtureHelperService.CreateFixture(
                        gameSaveId,
                        seasonId,
                        home.Id,
                        away.Id,
                        DateTime.MinValue,
                        1,
                        CompetitionType.DomesticCup,
                        firstRound
                    );

                    fixture.CupRound = firstRound;
                    fixtures.Add(fixture);
                }

                _context.CupRounds.Add(firstRound);
            }

            return fixtures;
        }

        private static bool NeedsPreliminaryRound(int teamCount)
        {
            return teamCount > 0 && (teamCount & teamCount - 1) != 0;
        }

        private static int GetTeamsInPrelimRound(int teamCount)
        {
            if (!NeedsPreliminaryRound(teamCount))
                return 0;

            int prevPower = (int)Math.Pow(2, Math.Floor(Math.Log(teamCount, 2)));
            return teamCount - prevPower;
        }

        public async Task GenerateNextRoundAsync(int cupId, int gameSaveId, int seasonId)
        {
            var cup = await _context.Cups
                .Include(c => c.Teams)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                .FirstOrDefaultAsync(c => c.Id == cupId);

            if (cup == null) return;

            var lastRound = cup.Rounds
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefault();

            if (lastRound == null) return;

            if (lastRound.Fixtures.Any(f => f.WinnerTeamId == null))
                return;

            var winners = lastRound.Fixtures
                .Select(f => f.WinnerTeamId!.Value)
                .ToList();

            var losers = lastRound.Fixtures
                .SelectMany(f => new[] { f.HomeTeamId, f.AwayTeamId })
                .Except(winners)
                .ToList();

            foreach (var ct in cup.Teams.Where(t => losers.Contains(t.TeamId)))
            {
                ct.IsEliminated = true;
            }

            await _context.SaveChangesAsync();

            if (winners.Count < 2)
            {
                var championId = winners.FirstOrDefault();
                Console.WriteLine($"🏆 Cup {cup.Id} Champion: {championId}");
                return;
            }

            var nextRound = new CupRound
            {
                CupId = cup.Id,
                RoundNumber = lastRound.RoundNumber + 1,
                Name = $"Round {lastRound.RoundNumber + 1}"
            };

            winners = winners.OrderBy(_ => Guid.NewGuid()).ToList();

            var fixtures = new List<Models.Fixtures.Fixture>();
            for (int i = 0; i < winners.Count; i += 2)
            {
                var home = winners[i];
                var away = winners[i + 1];

                var fixture = _fixtureHelperService.CreateFixture(
                    gameSaveId,
                    seasonId,
                    home,
                    away,
                    DateTime.MinValue,
                    1,
                    CompetitionType.DomesticCup,
                    nextRound
                );

                fixtures.Add(fixture);
            }

            nextRound.Fixtures = fixtures;
            _context.CupRounds.Add(nextRound);
            await _context.SaveChangesAsync();
        }
    }
}