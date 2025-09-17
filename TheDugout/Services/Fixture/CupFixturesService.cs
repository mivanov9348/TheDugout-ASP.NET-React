using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;
using TheDugout.Services.Season;

namespace TheDugout.Services.Fixture
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
            List<Models.Competitions.Cup> cups)
        {
            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null) return;

            var allFixtures = new List<Models.Matches.Fixture>();

            // 1) Подготвяме и разбъркваме отборите за всеки cup
            var cupTeamsMap = new Dictionary<int, List<TheDugout.Models.Teams.Team>>();
            foreach (var cup in cups)
            {
                var teams = cup.Teams
                    .Select(ct => new TheDugout.Models.Teams.Team
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

            // 2) Проверяваме дали глобално е нужен prelim
            bool anyNeedsPrelim = cupTeamsMap.Values.Any(t => NeedsPreliminaryRound(t.Count));

            // 3) Генерираме fixtures за всеки cup
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

            // 4) Сортираме/назначаваме дати и запазваме
            if (allFixtures.Any())
            {
                _cupScheduleService.AssignCupFixtures(
                    allFixtures,
                    season
                    //CompetitionType.DomesticCup
                );

                await _context.Fixtures.AddRangeAsync(allFixtures);
                await _context.SaveChangesAsync();
            }
        }
        private List<Models.Matches.Fixture> GenerateFixturesForCup(
 Models.Competitions.Cup cup,
 List<TheDugout.Models.Teams.Team> teams,
 int gameSaveId,
 int seasonId,
 bool globalNeedsPrelim)
        {
            var fixtures = new List<Models.Matches.Fixture>();
            bool needsPrelim = NeedsPreliminaryRound(teams.Count);
            int prelimTeamsCount = GetTeamsInPrelimRound(teams.Count);

            // ----------- PRELIMINARY ROUND -----------
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
                        DateTime.MinValue, // дата ще се сложи по-късно
                        1,
                        CompetitionType.DomesticCup,
                        prelimRound
                    );

                    fixture.CupRound = prelimRound;
                    fixtures.Add(fixture);
                }

                _context.CupRounds.Add(prelimRound);

                // ВАЖНО: тук приключваме → не теглим Round 1 сега
                return fixtures;
            }

            // ----------- FIRST ROUND (ако НЯМА прелим никъде) -----------
            if (globalNeedsPrelim)
            {
                // ако някой друг cup има прелим → този чака
                return fixtures;
            }

            var teamsForFirstRound = new List<TheDugout.Models.Teams.Team>(teams);

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
            return teamCount > 0 && (teamCount & (teamCount - 1)) != 0;
        }

        private static int GetTeamsInPrelimRound(int teamCount)
        {
            if (!NeedsPreliminaryRound(teamCount))
                return 0;

            int prevPower = (int)Math.Pow(2, Math.Floor(Math.Log(teamCount, 2)));
            return teamCount - prevPower;
        }
    }
}
