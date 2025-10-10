using Microsoft.EntityFrameworkCore;
using System.Linq;
using TheDugout.Data;
using TheDugout.Models.Cups;
using TheDugout.Models.Enums;
using TheDugout.Models.Fixtures;
using TheDugout.Services.Fixture;
using TheDugout.Services.Season;

namespace TheDugout.Services.Cup
{
    public class CupFixturesService : ICupFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixtureHelperService;
        private readonly ICupScheduleService _cupScheduleService;
        private readonly Random _random = new();

        public CupFixturesService(
            DugoutDbContext context,
            IFixturesHelperService fixtureHelperService,
            ICupScheduleService cupScheduleService)
        {
            _context = context;
            _fixtureHelperService = fixtureHelperService;
            _cupScheduleService = cupScheduleService;
        }

        public async Task GenerateInitialFixturesAsync(int seasonId, int gameSaveId, List<Models.Cups.Cup> cups)
        {
            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (season == null) return;

            var allFixtures = new List<Models.Fixtures.Fixture>();

            foreach (var cup in cups)
            {
                var activeTeams = cup.Teams
                    .Where(ct => !ct.IsEliminated)
                    .Select(ct => ct.Team)
                    .Where(t => t != null)
                    .Cast<Models.Teams.Team>()
                    .OrderBy(_ => _random.Next())
                    .ToList();

                if (activeTeams.Count < 2)
                    continue;

                var roundFixtures = GenerateFirstRoundFixtures(cup, activeTeams, gameSaveId, seasonId);
                allFixtures.AddRange(roundFixtures);
            }

            if (allFixtures.Any())
            {
                _cupScheduleService.AssignCupFixtures(allFixtures, season);
                await _context.Fixtures.AddRangeAsync(allFixtures);
                await _context.SaveChangesAsync();
            }
        }

        public async Task GenerateNextRoundAsync(int cupId, int gameSaveId, int? seasonId)
        {
            var cup = await _context.Cups
                .Where(c => c.Id == cupId && c.SeasonId == seasonId)
                .Include(c => c.Teams)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                .FirstOrDefaultAsync();

            if (cup == null) return;

            // филтрираме вече в паметта
            cup.Teams = cup.Teams.ToList();

            cup.Rounds = cup.Rounds
                .Where(r => r.Cup.SeasonId == seasonId)
                .ToList();

            var lastRound = cup.Rounds
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefault();

            if (lastRound == null) return;

            // Ако някой мач няма победител → не правим кръг
            if (lastRound.Fixtures.Any(f => f.WinnerTeamId == null))
                return;

            var winners = lastRound.Fixtures
                .Select(f => f.WinnerTeamId!.Value)
                .ToList();

            var losers = lastRound.Fixtures
                .SelectMany(f => new[] { f.HomeTeamId, f.AwayTeamId })
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Except(winners)
                .ToList();

            foreach (var ct in cup.Teams.Where(t => losers.Contains(t.TeamId ?? -1)))
            {
                ct.IsEliminated = true;
            }
            await _context.SaveChangesAsync();

            // Ако имаме само 1 победител → шампион
            if (winners.Count < 2 && cup.Teams.Count(t => !t.IsEliminated) == 1)
            {
                var championId = winners.FirstOrDefault();
                Console.WriteLine($"🏆 Cup {cup.Id} Champion: {championId}");
                return;
            }

            // Взимаме участниците за следващия кръг
            var nextRoundTeams = GetNextRoundTeams(cup, lastRound, winners);

            var totalRounds = (int)Math.Log2(cup.Teams.Count(t => !t.IsEliminated));
            var roundNumber = (lastRound.RoundNumber ?? 0) + 1;

            var roundName = _fixtureHelperService.GetRoundName(
                nextRoundTeams.Count,
                roundNumber,
                totalRounds
            );

            var nextRound = new CupRound
            {
                CupId = cup.Id,
                RoundNumber = lastRound.RoundNumber + 1,
                Name = roundName
            };

            var shuffledTeams = nextRoundTeams.OrderBy(_ => Guid.NewGuid()).ToList();
            var fixtures = new List<Models.Fixtures.Fixture>();

            for (int i = 0; i < shuffledTeams.Count; i += 2)
            {
                var homeId = shuffledTeams[i];
                var awayId = shuffledTeams[i + 1];

                var fixture = _fixtureHelperService.CreateFixture(
                    gameSaveId,
                    seasonId,
                    homeId,
                    awayId,
                    DateTime.MinValue,
                    1,
                    CompetitionTypeEnum.DomesticCup,
                    nextRound
                );

                fixtures.Add(fixture);
            }

            nextRound.Fixtures = fixtures;
            _context.CupRounds.Add(nextRound);
            await _context.SaveChangesAsync();
        }


        public bool IsRoundFinished(CupRound round)
        {
            return round.Fixtures.All(f => f.WinnerTeamId != null);
        }

        // ---------------------------
        // Helpers
        // ---------------------------

        private List<Models.Fixtures.Fixture> GenerateFirstRoundFixtures(
            Models.Cups.Cup cup,
            List<Models.Teams.Team> teams,
            int gameSaveId,
            int seasonId)
        {
            var fixtures = new List<Models.Fixtures.Fixture>();

            bool needsPrelim = NeedsPreliminaryRound(teams.Count);
            int prelimTeamsCount = GetTeamsInPrelimRound(teams.Count);

            if (needsPrelim && prelimTeamsCount > 0)
            {
                // Прелим рунд
                var prelimTeams = teams.Take(prelimTeamsCount * 2).ToList();
                var prelimRound = new CupRound
                {
                    CupId = cup.Id,
                    RoundNumber = 1,
                    Name = _fixtureHelperService.GetRoundName(teams.Count, 1, (int)Math.Log2(teams.Count), true)
                };

                fixtures.AddRange(PairTeamsIntoFixtures(prelimTeams, gameSaveId, seasonId, prelimRound));
                _context.CupRounds.Add(prelimRound);
            }
            else
            {
                // Няма прелим → директно първи кръг
                var round = new CupRound
                {
                    CupId = cup.Id,
                    RoundNumber = 1,
                    Name = _fixtureHelperService.GetRoundName(teams.Count, 1, (int)Math.Log2(teams.Count), false)
                };

                var roundTeams = teams;
                if (roundTeams.Count % 2 != 0)
                    roundTeams = roundTeams.Take(roundTeams.Count - 1).ToList();

                fixtures.AddRange(PairTeamsIntoFixtures(roundTeams, gameSaveId, seasonId, round));
                _context.CupRounds.Add(round);
            }

            return fixtures;
        }

        private List<Models.Fixtures.Fixture> PairTeamsIntoFixtures(List<Models.Teams.Team> teams, int gameSaveId, int seasonId, CupRound round)
        {
            var fixtures = new List<Models.Fixtures.Fixture>();

            for (int i = 0; i < teams.Count; i += 2)
            {
                var home = teams[i];
                var away = teams[i + 1];

                var fixture = _fixtureHelperService.CreateFixture(
                    gameSaveId,
                    seasonId,
                    home.Id,
                    away.Id,
                    DateTime.MinValue,
                    1,
                    CompetitionTypeEnum.DomesticCup,
                    round
                );

                fixture.CupRound = round;
                fixtures.Add(fixture);
            }

            return fixtures;
        }

        private static bool NeedsPreliminaryRound(int teamCount)
            => teamCount > 0 && (teamCount & (teamCount - 1)) != 0;

        private static int GetTeamsInPrelimRound(int teamCount)
        {
            if (!NeedsPreliminaryRound(teamCount))
                return 0;

            int prevPower = (int)Math.Pow(2, Math.Floor(Math.Log(teamCount, 2)));
            return teamCount - prevPower;
        }

        private static List<int> GetNextRoundTeams(Models.Cups.Cup cup, CupRound lastRound, List<int> winners)
        {
            if (lastRound.Name.Contains("Preliminary", StringComparison.OrdinalIgnoreCase))
            {
                var prelimTeams = lastRound.Fixtures
                    .SelectMany(f => new[] { f.HomeTeamId, f.AwayTeamId })
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                var waitingTeams = cup.Teams
                    .Where(t => !t.IsEliminated && !prelimTeams.Contains(t.TeamId ?? -1))
                    .Select(t => t.TeamId ?? -1)
                    .ToList();

                return winners.Concat(waitingTeams).ToList();
            }

            return winners;
        }


    }
}
