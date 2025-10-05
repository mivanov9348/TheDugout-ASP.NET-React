
using Microsoft.EntityFrameworkCore;
using System;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Fixtures;
using TheDugout.Services.Fixture;
using TheDugout.Services.Season;

namespace TheDugout.Services.EuropeanCup
{
    public class EurocupFixturesService : IEurocupFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly IEurocupScheduleService _eurocupScheduleService;
        private readonly ILogger<EurocupFixturesService> _logger;
        private readonly Random _random = new();

        public EurocupFixturesService(DugoutDbContext context, IFixturesHelperService fixturesHelperService, ILogger<EurocupFixturesService> logger, IEurocupScheduleService eurocupScheduleService)
        {
            _context = context;
            _fixturesHelperService = fixturesHelperService;
            _eurocupScheduleService = eurocupScheduleService;
            _logger = logger;
        }

        public async Task GenerateEuropeanLeaguePhaseFixturesAsync(
    int europeanCupId,
    int seasonId,
    CancellationToken ct = default)
        {
            var cup = await _context.Set<Models.Competitions.EuropeanCup>()
                .Include(x => x.Template)
                .Include(x => x.Teams)
                .Include(x => x.Phases).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(x => x.Id == europeanCupId, ct)
                ?? throw new InvalidOperationException($"Cup {europeanCupId} not found.");

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId, ct)
                ?? throw new InvalidOperationException("Season not found.");

            var leaguePhase = cup.Phases
                               .FirstOrDefault(p => p.PhaseTemplate.Order == 1)
                               ?? throw new InvalidOperationException("No league phase found.");


            int rounds = cup.Template.LeaguePhaseMatchesPerTeam;
            var teamIds = cup.Teams.Select(t => t.TeamId).ToList();

            var existingPairs = new HashSet<string>();
            var existing = await _context.Set<Models.Fixtures.Fixture>()
                .Where(f => f.EuropeanCupPhaseId == leaguePhase.Id)
                .ToListAsync(ct);

            foreach (var f in existing)
                existingPairs.Add(_fixturesHelperService.PairKey(f.HomeTeamId, f.AwayTeamId));

            var homeCount = teamIds.ToDictionary(id => id, _ => 0);
            var fixturesToAdd = new List<Models.Fixtures.Fixture>();

            // Взимаме равномерно разпределени дати от календара
            var roundDates = _eurocupScheduleService.AssignEuropeanFixtures(season, rounds);

            for (int round = 1; round <= rounds; round++)
            {
                var roundPairs = _fixturesHelperService.TryFindRoundPairing(teamIds, existingPairs, 2000)
                                 ?? _fixturesHelperService.GreedyPairingMinimizeRepeats(teamIds, existingPairs);

                foreach (var (a, b) in roundPairs)
                {
                    int home = _fixturesHelperService.DecideHome(a, b, homeCount);
                    int away = home == a ? b : a;

                    fixturesToAdd.Add(_fixturesHelperService.CreateFixture(
                        cup.GameSaveId,
                        seasonId,
                        home,
                        away,
                        roundDates[Math.Min(round - 1, roundDates.Count - 1)], // <-- вече от календара
                        round,
                        CompetitionType.EuropeanCup,
                        europeanCupPhaseId: leaguePhase.Id
                    ));

                    existingPairs.Add(_fixturesHelperService.PairKey(a, b));
                    homeCount[home]++;
                }
            }

            await _context.AddRangeAsync(fixturesToAdd, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Generated {Count} league fixtures for cup {CupId}", fixturesToAdd.Count, cup.Id);
        }

    }
}
