
using Microsoft.EntityFrameworkCore;
using System;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Matches;
using TheDugout.Services.Season;

namespace TheDugout.Services.Fixture
{
    public class EurocupFixturesService : IEurocupFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly ISeasonCalendarService _seasonCalendarService;
        private readonly ILogger<EurocupFixturesService> _logger;
        private readonly Random _random = new();


        public EurocupFixturesService(DugoutDbContext context, IFixturesHelperService fixturesHelperService, ILogger<EurocupFixturesService> logger, ISeasonCalendarService seasonCalendarService)
        {
            _context = context;
            _fixturesHelperService = fixturesHelperService;
            _seasonCalendarService = seasonCalendarService;
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

            var leaguePhase = cup.Phases.FirstOrDefault(p => !p.PhaseTemplate.IsKnockout)
                ?? throw new InvalidOperationException("No league phase found.");

            int rounds = cup.Template.LeaguePhaseMatchesPerTeam;
            var teamIds = cup.Teams.Select(t => t.TeamId).ToList();

            var existingPairs = new HashSet<string>();
            var existing = await _context.Set<Models.Matches.Fixture>()
                .Where(f => f.EuropeanCupPhaseId == leaguePhase.Id)
                .ToListAsync(ct);

            foreach (var f in existing)
                existingPairs.Add(_fixturesHelperService.PairKey(f.HomeTeamId, f.AwayTeamId));

            var homeCount = teamIds.ToDictionary(id => id, _ => 0);
            var fixturesToAdd = new List<Models.Matches.Fixture>();

            // Взимаме равномерно разпределени дати от календара
            var roundDates = _seasonCalendarService.DistributeEuropeanRounds(season, rounds);

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


        // ---------------- EUROCUP: KNOCKOUT ----------------
        public async Task GenerateEuropeanKnockoutFixturesAsync(int europeanCupId, int knockoutPhaseTemplateId, CancellationToken ct = default)
        {
            var phaseTemplate = await _context.Set<EuropeanCupPhaseTemplate>()
                .FirstOrDefaultAsync(p => p.Id == knockoutPhaseTemplateId, ct)
                ?? throw new InvalidOperationException("Phase template not found.");

            var phase = await _context.Set<EuropeanCupPhase>()
                .FirstOrDefaultAsync(p => p.EuropeanCupId == europeanCupId && p.PhaseTemplateId == knockoutPhaseTemplateId, ct)
                ?? throw new InvalidOperationException("Phase not found.");

            var top16 = await _context.Set<EuropeanCupStanding>()
                .Where(s => s.EuropeanCupId == europeanCupId)
                .OrderBy(s => s.Ranking)
                .Take(16)
                .Select(s => s.TeamId)
                .ToListAsync(ct);

            if (top16.Count != 16)
                throw new InvalidOperationException($"Expect 16 teams to generate knockout, got {top16.Count}.");

            var shuffled = top16.OrderBy(_ => _random.Next()).ToList();
            var fixturesToAdd = new List<Models.Matches.Fixture>();

            int gameSaveId = await _context.Set<Models.Competitions.EuropeanCup>()
                .Where(c => c.Id == europeanCupId)
                .Select(c => c.GameSaveId)
                .FirstAsync(ct);

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                int a = shuffled[i];
                int b = shuffled[i + 1];

                if (phaseTemplate.IsTwoLegged)
                {
                    fixturesToAdd.Add(_fixturesHelperService.CreateFixture(gameSaveId, seasonId: phase.EuropeanCup.SeasonId, a, b,
                        DateTime.UtcNow.AddDays(7 + (i / 2) * 14), 1, CompetitionType.EuropeanCup, europeanCupPhaseId: phase.Id));

                    fixturesToAdd.Add(_fixturesHelperService.CreateFixture(gameSaveId, seasonId: phase.EuropeanCup.SeasonId, b, a,
                        DateTime.UtcNow.AddDays(14 + (i / 2) * 14), 2, CompetitionType.EuropeanCup, europeanCupPhaseId: phase.Id));
                }
                else
                {
                    fixturesToAdd.Add(_fixturesHelperService.CreateFixture(gameSaveId, seasonId: phase.EuropeanCup.SeasonId, a, b,
                        DateTime.UtcNow.AddDays(7 + (i / 2) * 7), 1, CompetitionType.EuropeanCup, europeanCupPhaseId: phase.Id));
                }
            }

            await _context.AddRangeAsync(fixturesToAdd, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Generated {Count} knockout fixtures for European cup {CupId}", fixturesToAdd.Count, europeanCupId);
        }
    }
}
