
using Microsoft.EntityFrameworkCore;
using System;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Enums;
using TheDugout.Models.Fixtures;
using TheDugout.Services.EuropeanCup.Interfaces;
using TheDugout.Services.Fixture;
using TheDugout.Services.Season.Interfaces;

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
                .Include(x => x.Template).ThenInclude(t => t.PhaseTemplates)
                .Include(x => x.Teams)
                .Include(x => x.Phases).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(x => x.Id == europeanCupId, ct)
                ?? throw new InvalidOperationException($"Cup {europeanCupId} not found.");

            // Guard: template must be active
            if (!cup.Template.IsActive)
            {
                _logger.LogWarning("Cup {CupId} uses template {TemplateId} which is not active - aborting fixture generation.", cup.Id, cup.TemplateId);
                return;
            }

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId, ct)
                ?? throw new InvalidOperationException("Season not found.");

            // safer: търсим league phase във вече заредените cup.Phases
            var leaguePhase = cup.Phases.FirstOrDefault(p => p.PhaseTemplate != null && p.PhaseTemplate.Order == 1);
            if (leaguePhase == null)
            {
                _logger.LogError("League phase not found for cup {CupId}. Cup.Phases: {Phases}. Template.Phases: {TemplatePhases}",
                    cup.Id,
                    string.Join(",", cup.Phases.Select(p => $"{p.Id}:{p.PhaseTemplateId}->{p.PhaseTemplate?.Order ?? -1}")),
                    string.Join(",", cup.Template?.PhaseTemplates?.Select(pt => $"{pt.Id}:{pt.Order}") ?? Enumerable.Empty<string>())
                );
                throw new InvalidOperationException("League phase not found.");
            }

            int rounds = cup.Template.LeaguePhaseMatchesPerTeam;
            if (rounds <= 0)
            {
                _logger.LogError("Invalid number of rounds ({Rounds}) in template for cup {CupId}", rounds, cup.Id);
                return;
            }

            var teamIds = cup.Teams
                .Select(t => t.TeamId)
                .Where(id => id.HasValue) 
                .Select(id => id.Value)   
                .ToList();
            
            if (teamIds.Count == 0)
            {
                _logger.LogError("No teams attached to cup {CupId} - cannot generate fixtures.", cup.Id);
                return;
            }

            var existingPairs = new HashSet<string>();
            var existing = await _context.Set<Models.Fixtures.Fixture>()
                .Where(f => f.EuropeanCupPhaseId == leaguePhase.Id)
                .ToListAsync(ct);

            foreach (var f in existing)
                existingPairs.Add(_fixturesHelperService.PairKey(f.HomeTeam.Id, f.AwayTeam.Id));

            var homeCount = teamIds.ToDictionary(id => id, _ => 0);
            var fixturesToAdd = new List<Models.Fixtures.Fixture>();

            var roundDates = _eurocupScheduleService.AssignEuropeanFixtures(season, rounds);

            for (int round = 1; round <= rounds; round++)
            {
                var usedTeams = new HashSet<int>();

                var roundPairs = _fixturesHelperService.TryFindRoundPairing(teamIds, existingPairs, 2000)
                                     ?? _fixturesHelperService.GreedyPairingMinimizeRepeats(teamIds, existingPairs);

                foreach (var (a, b) in roundPairs)
                {
                    if (usedTeams.Contains(a) || usedTeams.Contains(b))
                        continue;

                    int home = _fixturesHelperService.DecideHome(a, b, homeCount);
                    int away = home == a ? b : a;

                    fixturesToAdd.Add(_fixturesHelperService.CreateFixture(
                        cup.GameSaveId,
                        seasonId,
                        home,
                        away,
                        roundDates[Math.Min(round - 1, roundDates.Count - 1)],
                        round,
                        CompetitionTypeEnum.EuropeanCup,
                        europeanCupPhaseId: leaguePhase.Id
                    ));

                    usedTeams.Add(a);
                    usedTeams.Add(b);

                    existingPairs.Add(_fixturesHelperService.PairKey(a, b));
                    homeCount[home]++;
                }
            }

            await _context.AddRangeAsync(fixturesToAdd, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Generated {Count} league fixtures for cup {CupId}", fixturesToAdd.Count, cup.Id);
        }

        public async Task<List<Models.Fixtures.Fixture>> GetAllFixturesForCupAsync(int europeanCupId)
        {
            var phaseIds = await _context.EuropeanCupPhases
                .Where(p => p.EuropeanCupId == europeanCupId)
                .Select(p => p.Id)
                .ToListAsync();

            return await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Where(f => f.EuropeanCupPhaseId.HasValue && phaseIds.Contains(f.EuropeanCupPhaseId.Value))
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetGroupFixturesAsync(int europeanCupId)
        {
            var groupPhase = await _context.EuropeanCupPhases
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.HomeTeam)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.AwayTeam)
                .FirstOrDefaultAsync(p => p.EuropeanCupId == europeanCupId && !p.PhaseTemplate.IsKnockout);

            if (groupPhase == null)
                return Enumerable.Empty<object>();

            return groupPhase.Fixtures
                .GroupBy(f => f.Round)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    round = g.Key,
                    matches = g.Select(f => new
                    {
                        id = f.Id,
                        homeTeam = f.HomeTeam == null ? null : new
                        {
                            id = f.HomeTeam.Id,
                            name = f.HomeTeam.Name,
                            logoFileName = f.HomeTeam.LogoFileName
                        },
                        awayTeam = f.AwayTeam == null ? null : new
                        {
                            id = f.AwayTeam.Id,
                            name = f.AwayTeam.Name,
                            logoFileName = f.AwayTeam.LogoFileName
                        },
                        homeTeamGoals = f.HomeTeamGoals,
                        awayTeamGoals = f.AwayTeamGoals,
                        date = f.Date,
                        status = f.Status
                    })
                });
        }
    }
}
