using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;

namespace TheDugout.Services.EuropeanCup
{
    public class EuropeanCupService : IEuropeanCupService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<EuropeanCupService> _logger;
        private readonly Random _rng = new Random();
        public EuropeanCupService(DugoutDbContext context, ILogger<EuropeanCupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Models.EuropeanCup> InitializeTournamentAsync(
    int templateId,
    int gameSaveId,
    int seasonId,
    CancellationToken ct = default)
        {
            // 1. Зареждаме шаблона и фазите
            var template = await _context.Set<EuropeanCupTemplate>()
                .Include(t => t.PhaseTemplates)
                .FirstOrDefaultAsync(t => t.Id == templateId, ct)
                ?? throw new InvalidOperationException($"Template {templateId} not found.");

            var teams = await _context.Set<Models.Team>().ToListAsync();

            // 2. Взимаме eligible отбори (LeagueId == null и същия GameSave)
            var eligibleTeams = await _context.Set<Models.Team>()
                .Where(t => t.LeagueId == null)
                .ToListAsync(ct);

            if (eligibleTeams.Count < template.TeamsCount)
                throw new InvalidOperationException(
                    $"Not enough eligible teams ({eligibleTeams.Count}) for template requires {template.TeamsCount}.");

            // 3. Рандъмизираме избора
            var chosenTeams = eligibleTeams
    .OrderBy(_ => _rng.Next()) 
    .Take(template.TeamsCount)
    .ToList();

            // 4. Създаваме Cup
            var cup = new Models.EuropeanCup
            {
                TemplateId = template.Id,
                GameSaveId = gameSaveId,
                SeasonId = seasonId
            };

            _context.Add(cup);
            await _context.SaveChangesAsync(ct); // Id нужен за FK

            // 5. Добавяме отбори + standings с ПРЕДВАРИТЕЛЕН РАНКИНГ ПО ПОПУЛЯРНОСТ!
            var rankedTeams = chosenTeams
                .OrderByDescending(t => t.Popularity)
                .ThenByDescending(t => t.Id)                    
                .ThenBy(t => t.Country?.Name ?? "")             
                .ThenBy(t => t.Name)                            
                .ToList();

            for (int i = 0; i < rankedTeams.Count; i++)
            {
                var team = rankedTeams[i];

                _context.Add(new EuropeanCupTeam
                {
                    EuropeanCupId = cup.Id,
                    TeamId = team.Id
                });

                _context.Add(new EuropeanCupStanding
                {
                    EuropeanCupId = cup.Id,
                    TeamId = team.Id,
                    Points = 0,
                    Matches = 0,
                    Wins = 0,
                    Draws = 0,
                    Losses = 0,
                    GoalsFor = 0,
                    GoalsAgainst = 0,
                    GoalDifference = 0,
                    Ranking = i + 1 
                });
            }

            // 6. Фази по ред
            var orderedPhaseTemplates = template.PhaseTemplates.OrderBy(p => p.Order).ToList();
            foreach (var pt in orderedPhaseTemplates)
            {
                _context.Add(new EuropeanCupPhase
                {
                    EuropeanCupId = cup.Id,
                    PhaseTemplateId = pt.Id
                });
            }

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Initialized EuropeanCup {CupId} with {Teams} teams", cup.Id, chosenTeams.Count);

            return cup;
        }
        public async Task GenerateLeaguePhaseFixturesAsync(
    int europeanCupId,
    int seasonId,
    CancellationToken ct = default)
        {
            // 1. Зареждаме купата
            var cup = await _context.Set<Models.EuropeanCup>()
                .Include(x => x.Template)
                .Include(x => x.Teams)
                .Include(x => x.Phases).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(x => x.Id == europeanCupId, ct)
                ?? throw new InvalidOperationException($"Cup {europeanCupId} not found.");

            // 2. Намираме league фаза
            var leaguePhase = cup.Phases
                .FirstOrDefault(p => !p.PhaseTemplate.IsKnockout)
                ?? throw new InvalidOperationException("No league phase template found for this cup.");

            int rounds = cup.Template.LeaguePhaseMatchesPerTeam; // 8
            var teamIds = cup.Teams.Select(t => t.TeamId).ToList();

            // 3. Проверяваме за вече генерирани fixtures
            var existingPairs = new HashSet<string>();
            var existing = await _context.Set<Models.Fixture>()
                .Where(f => f.EuropeanCupPhaseId == leaguePhase.Id)
                .ToListAsync(ct);
            foreach (var f in existing)
                existingPairs.Add(PairKey(f.HomeTeamId, f.AwayTeamId));

            var homeCount = teamIds.ToDictionary(id => id, id => 0);
            var fixturesToAdd = new List<Models.Fixture>();

            // 4. Генерираме всеки рунд
            for (int round = 1; round <= rounds; round++)
            {
                var roundPairs = TryFindRoundPairing(teamIds, existingPairs, maxAttempts: 2000)
                                 ?? GreedyPairingMinimizeRepeats(teamIds, existingPairs);

                foreach (var (a, b) in roundPairs)
                {
                    int home = DecideHome(a, b, homeCount);
                    int away = home == a ? b : a;

                    fixturesToAdd.Add(new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = leaguePhase.Id,
                        HomeTeamId = home,
                        AwayTeamId = away,
                        Date = DateTime.UtcNow.AddDays(round * 7),
                        Round = round,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = cup.GameSaveId,
                        SeasonId = seasonId
                    });

                    existingPairs.Add(PairKey(a, b));
                    homeCount[home]++;
                }
            }

            await _context.AddRangeAsync(fixturesToAdd, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Generated {Count} league fixtures for cup {CupId}", fixturesToAdd.Count, cup.Id);
        }

        private List<(int, int)> TryFindRoundPairing(List<int> teamIds, HashSet<string> existingPairs, int maxAttempts)
        {
            // attempt shuffles to find a pairing where none of the pairs exist in existingPairs
            var rnd = _rng;
            var n = teamIds.Count;
            if (n % 2 != 0) throw new InvalidOperationException("Team count must be even for pairing.");

            var working = teamIds.ToArray();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Fisher-Yates shuffle
                for (int i = n - 1; i > 0; i--)
                {
                    int j = rnd.Next(i + 1);
                    var tmp = working[i];
                    working[i] = working[j];
                    working[j] = tmp;
                }

                bool ok = true;
                var pairs = new List<(int, int)>(n / 2);
                for (int i = 0; i < n; i += 2)
                {
                    var a = working[i];
                    var b = working[i + 1];
                    if (existingPairs.Contains(PairKey(a, b)))
                    {
                        ok = false;
                        break;
                    }
                    pairs.Add((a, b));
                }

                if (ok) return pairs;
            }

            return null; // no perfect pairing found
        }

        private List<(int, int)> GreedyPairingMinimizeRepeats(List<int> teamIds, HashSet<string> existingPairs)
        {
            // Greedy: for each team, try to pick opponent not used yet, else accept least-worse
            var remaining = new HashSet<int>(teamIds);
            var pairs = new List<(int, int)>();
            while (remaining.Count > 0)
            {
                int a = remaining.First();
                remaining.Remove(a);

                int best = -1;
                foreach (var cand in remaining)
                {
                    if (!existingPairs.Contains(PairKey(a, cand)))
                    {
                        best = cand;
                        break;
                    }
                }
                if (best == -1)
                {
                    // all remaining have been seen with a — pick random one (minimize future harm)
                    best = remaining.First();
                }

                remaining.Remove(best);
                pairs.Add((a, best));
                // do NOT add to existingPairs here — caller will add after acceptance
            }
            return pairs;
        }

        private string PairKey(int a, int b)
        {
            if (a < b) return $"{a}:{b}";
            return $"{b}:{a}";
        }

        private int DecideHome(int a, int b, Dictionary<int, int> homeCount)
        {
            var aHome = homeCount.TryGetValue(a, out var ac) ? ac : 0;
            var bHome = homeCount.TryGetValue(b, out var bc) ? bc : 0;
            if (aHome < bHome) return a;
            if (bHome < aHome) return b;
            // equal -> random
            return _rng.Next(2) == 0 ? a : b;
        }

        public async Task CreateStandingsIfNotExistsAsync(int europeanCupId, CancellationToken ct = default)
        {
            var cup = await _context.Set<Models.EuropeanCup>()
                               .Include(c => c.Teams)
                               .FirstOrDefaultAsync(c => c.Id == europeanCupId, ct)
                      ?? throw new InvalidOperationException($"Cup {europeanCupId} not found.");

            var existing = await _context.Set<EuropeanCupStanding>()
                                    .Where(s => s.EuropeanCupId == europeanCupId)
                                    .Select(s => s.TeamId)
                                    .ToListAsync(ct);

            var toAdd = cup.Teams.Where(t => !existing.Contains(t.TeamId))
                                 .Select(t => new EuropeanCupStanding
                                 {
                                     EuropeanCupId = europeanCupId,
                                     TeamId = t.TeamId
                                 }).ToList();

            if (toAdd.Any())
            {
                await _context.AddRangeAsync(toAdd, ct);
                await _context.SaveChangesAsync(ct);
            }
        }
        public async Task RecordFixtureResultAsync(int fixtureId, int homeGoals, int awayGoals, CancellationToken ct = default)
        {
            var fixture = await _context.Set<Models.Fixture>()
                                   .Include(f => f.HomeTeam)
                                   .Include(f => f.AwayTeam)
                                   .FirstOrDefaultAsync(f => f.Id == fixtureId, ct)
                           ?? throw new InvalidOperationException($"Fixture {fixtureId} not found.");

            if (fixture.Status == MatchStatus.Played)
                throw new InvalidOperationException("Fixture already played.");

            fixture.HomeTeamGoals = homeGoals;
            fixture.AwayTeamGoals = awayGoals;
            fixture.Status = MatchStatus.Played;
            _context.Update(fixture);
            await _context.SaveChangesAsync(ct);

            // update standings for the cup
            if (fixture.EuropeanCupPhaseId.HasValue)
            {
                await UpdateStandingsAfterFixtureAsync(fixture, ct);
            }
        }

        private async Task UpdateStandingsAfterFixtureAsync(Models.Fixture fixture, CancellationToken ct = default)
        {
            // Called after saving fixture
            var cupPhase = fixture.EuropeanCupPhaseId.HasValue ? fixture.EuropeanCupPhaseId.Value : 0;
            // find the cup id via phase
            var phase = await _context.Set<EuropeanCupPhase>().FirstOrDefaultAsync(p => p.Id == cupPhase, ct)
                        ?? throw new InvalidOperationException("Phase not found.");

            var cupId = phase.EuropeanCupId;

            var homeStanding = await _context.Set<EuropeanCupStanding>()
                                       .FirstOrDefaultAsync(s => s.EuropeanCupId == cupId && s.TeamId == fixture.HomeTeamId, ct)
                               ?? throw new InvalidOperationException("Home standing not found.");

            var awayStanding = await _context.Set<EuropeanCupStanding>()
                                       .FirstOrDefaultAsync(s => s.EuropeanCupId == cupId && s.TeamId == fixture.AwayTeamId, ct)
                               ?? throw new InvalidOperationException("Away standing not found.");

            // update matches/goals
            homeStanding.Matches++;
            awayStanding.Matches++;

            homeStanding.GoalsFor += fixture.HomeTeamGoals ?? 0;
            homeStanding.GoalsAgainst += fixture.AwayTeamGoals ?? 0;
            awayStanding.GoalsFor += fixture.AwayTeamGoals ?? 0;
            awayStanding.GoalsAgainst += fixture.HomeTeamGoals ?? 0;

            homeStanding.GoalDifference = homeStanding.GoalsFor - homeStanding.GoalsAgainst;
            awayStanding.GoalDifference = awayStanding.GoalsFor - awayStanding.GoalsAgainst;

            if (fixture.HomeTeamGoals > fixture.AwayTeamGoals)
            {
                homeStanding.Wins++;
                awayStanding.Losses++;
                homeStanding.Points += 3;
            }
            else if (fixture.HomeTeamGoals < fixture.AwayTeamGoals)
            {
                awayStanding.Wins++;
                homeStanding.Losses++;
                awayStanding.Points += 3;
            }
            else
            {
                homeStanding.Draws++;
                awayStanding.Draws++;
                homeStanding.Points += 1;
                awayStanding.Points += 1;
            }

            _context.UpdateRange(homeStanding, awayStanding);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateStandingsForPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default)
        {
            // Recalculate standings from scratch for a cup/phase's fixtures (useful for recompute)
            var phase = await _context.Set<EuropeanCupPhase>()
                                 .Include(p => p.EuropeanCup)
                                 .FirstOrDefaultAsync(p => p.Id == europeanCupPhaseId, ct)
                       ?? throw new InvalidOperationException($"Phase {europeanCupPhaseId} not found.");

            int cupId = phase.EuropeanCupId;

            // reset standings
            var standings = await _context.Set<EuropeanCupStanding>().Where(s => s.EuropeanCupId == cupId).ToListAsync(ct);
            foreach (var s in standings)
            {
                s.Points = s.Matches = s.Wins = s.Draws = s.Losses = s.GoalsFor = s.GoalsAgainst = s.GoalDifference = 0;
            }
            _context.UpdateRange(standings);
            await _context.SaveChangesAsync(ct);

            // get all played fixtures for that cup's league-phase(s) (only league-phase fixtures affect league standings)
            var fixtures = await _context.Set<Models.Fixture>()
                                    .Where(f => f.EuropeanCupPhaseId == europeanCupPhaseId && f.Status == MatchStatus.Played)
                                    .ToListAsync(ct);

            foreach (var f in fixtures)
            {
                var home = standings.First(s => s.TeamId == f.HomeTeamId);
                var away = standings.First(s => s.TeamId == f.AwayTeamId);

                home.Matches++;
                away.Matches++;

                home.GoalsFor += f.HomeTeamGoals ?? 0;
                home.GoalsAgainst += f.AwayTeamGoals ?? 0;
                away.GoalsFor += f.AwayTeamGoals ?? 0;
                away.GoalsAgainst += f.HomeTeamGoals ?? 0;

                if ((f.HomeTeamGoals ?? 0) > (f.AwayTeamGoals ?? 0))
                {
                    home.Wins++;
                    away.Losses++;
                    home.Points += 3;
                }
                else if ((f.HomeTeamGoals ?? 0) < (f.AwayTeamGoals ?? 0))
                {
                    away.Wins++;
                    home.Losses++;
                    away.Points += 3;
                }
                else
                {
                    home.Draws++;
                    away.Draws++;
                    home.Points += 1;
                    away.Points += 1;
                }
            }

            foreach (var s in standings)
            {
                s.GoalDifference = s.GoalsFor - s.GoalsAgainst;
            }

            // ranking: Points desc, GD desc, GF desc, Wins desc, random tie-break
            var rnd = _rng;
            var ranked = standings
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .ThenByDescending(s => s.Wins)
                .ThenBy(_ => rnd.Next())
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
            {
                ranked[i].Ranking = i + 1;
            }

            _context.UpdateRange(ranked);
            await _context.SaveChangesAsync(ct);
        }
        public async Task ProgressToNextPhaseIfReadyAsync(int europeanCupId, CancellationToken ct = default)
        {
            // check if league phase fixtures all played -> then compute standings and create knockout fixtures
            var cup = await _context.Set<Models.EuropeanCup>()
                               .Include(c => c.Template)
                               .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate)
                               .FirstOrDefaultAsync(c => c.Id == europeanCupId, ct)
                      ?? throw new InvalidOperationException("Cup not found.");

            var leaguePhase = cup.Phases.FirstOrDefault(p => !p.PhaseTemplate.IsKnockout)
                              ?? throw new InvalidOperationException("No league phase.");

            // check if any scheduled/unplayed fixture exists
            var anyUnplayed = await _context.Set<Models.Fixture>()
                                       .AnyAsync(f => f.EuropeanCupPhaseId == leaguePhase.Id && f.Status != MatchStatus.Played, ct);

            if (anyUnplayed) return; // not ready

            // recompute standings for league phase
            await UpdateStandingsForPhaseAsync(leaguePhase.Id, ct);

            // take top 16 and create knockout
            var standings = await _context.Set<EuropeanCupStanding>()
                                    .Where(s => s.EuropeanCupId == europeanCupId)
                                    .OrderBy(s => s.Ranking)
                                    .Take(16)
                                    .ToListAsync(ct);

            // find next knockout phase template (first IsKnockout true)
            var nextKnockoutTemplate = cup.Template.PhaseTemplates.OrderBy(p => p.Order).FirstOrDefault(p => p.IsKnockout)
                                        ?? throw new InvalidOperationException("No knockout phase template defined.");

            await GenerateKnockoutFixturesAsync(europeanCupId, nextKnockoutTemplate.Id, ct);
        }

        public async Task GenerateKnockoutFixturesAsync(int europeanCupId, int knockoutPhaseTemplateId, CancellationToken ct = default)
        {
            // Load target phase template and corresponding phase instance
            var phaseTemplate = await _context.Set<EuropeanCupPhaseTemplate>()
                                         .FirstOrDefaultAsync(p => p.Id == knockoutPhaseTemplateId, ct)
                              ?? throw new InvalidOperationException("Phase template not found.");

            var phase = await _context.Set<EuropeanCupPhase>()
                                 .FirstOrDefaultAsync(p => p.EuropeanCupId == europeanCupId && p.PhaseTemplateId == knockoutPhaseTemplateId, ct)
                        ?? throw new InvalidOperationException("Phase instance not found for this cup/template.");

            // get top 16 teams from standings
            var top16 = await _context.Set<EuropeanCupStanding>()
                                 .Where(s => s.EuropeanCupId == europeanCupId)
                                 .OrderBy(s => s.Ranking)
                                 .Take(16)
                                 .Select(s => s.TeamId)
                                 .ToListAsync(ct);

            if (top16.Count != 16)
                throw new InvalidOperationException($"Expect 16 teams to generate knockout, got {top16.Count}.");

            // pairing strategy: random shuffle then pair sequentially (can change to seeded)
            var shuffled = top16.OrderBy(_ => _rng.Next()).ToList();

            var fixturesToAdd = new List<Models.Fixture>();
            for (int i = 0; i < shuffled.Count; i += 2)
            {
                int a = shuffled[i];
                int b = shuffled[i + 1];

                if (phaseTemplate.IsTwoLegged)
                {
                    // two fixtures: a home vs b, then b home vs a
                    var f1 = new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = phase.Id,
                        HomeTeamId = a,
                        AwayTeamId = b,
                        Date = DateTime.UtcNow.AddDays(7 + (i / 2) * 14),
                        Round = 1,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = (await _context.Set<Models.EuropeanCup>().Where(c => c.Id == europeanCupId).Select(c => c.GameSaveId).FirstAsync(ct))
                    };
                    var f2 = new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = phase.Id,
                        HomeTeamId = b,
                        AwayTeamId = a,
                        Date = f1.Date.AddDays(7), // return leg
                        Round = 2,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = f1.GameSaveId,
                    };
                    fixturesToAdd.Add(f1);
                    fixturesToAdd.Add(f2);
                }
                else
                {
                    var f = new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = phase.Id,
                        HomeTeamId = a,
                        AwayTeamId = b,
                        Date = DateTime.UtcNow.AddDays(7 + (i / 2) * 7),
                        Round = 1,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = (await _context.Set<Models.EuropeanCup>().Where(c => c.Id == europeanCupId).Select(c => c.GameSaveId).FirstAsync(ct))
                    };
                    fixturesToAdd.Add(f);
                }
            }

            await _context.AddRangeAsync(fixturesToAdd, ct);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Generated {Count} knockout fixtures for phase {PhaseId}", fixturesToAdd.Count, phase.Id);
        }
        public async Task ResolveKnockoutPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default)
        {
            var phase = await _context.Set<EuropeanCupPhase>()
                                 .Include(p => p.PhaseTemplate)
                                 .FirstOrDefaultAsync(p => p.Id == europeanCupPhaseId, ct)
                        ?? throw new InvalidOperationException("Phase not found.");

            if (!phase.PhaseTemplate.IsKnockout) throw new InvalidOperationException("Phase is not knockout.");

            // if two-legged combine pairs by matching unordered pair
            var fixtures = await _context.Set<Models.Fixture>()
                                    .Where(f => f.EuropeanCupPhaseId == europeanCupPhaseId && f.Status == MatchStatus.Played)
                                    .ToListAsync(ct);

            var winners = new List<int>();
            if (phase.PhaseTemplate.IsTwoLegged)
            {
                // group fixtures by unordered pair key
                var grouped = fixtures.GroupBy(f => PairKey(f.HomeTeamId, f.AwayTeamId));
                foreach (var g in grouped)
                {
                    // pick the two fixtures, compute aggregate
                    var gf = g.ToList();
                    if (gf.Count < 1) continue;
                    if (gf.Count == 1)
                    {
                        // single leg played only -> winner is winner of that fixture
                        var f = gf[0];
                        if ((f.HomeTeamGoals ?? 0) > (f.AwayTeamGoals ?? 0)) winners.Add(f.HomeTeamId);
                        else if ((f.HomeTeamGoals ?? 0) < (f.AwayTeamGoals ?? 0)) winners.Add(f.AwayTeamId);
                        else winners.Add(_rng.Next(2) == 0 ? f.HomeTeamId : f.AwayTeamId); // penalty/random
                        continue;
                    }

                    // find both legs supposing one is home A vs B and other is home B vs A
                    // We compute aggregate for the unordered pair
                    int teamA = Math.Min(gf[0].HomeTeamId, gf[0].AwayTeamId);
                    int teamB = Math.Max(gf[0].HomeTeamId, gf[0].AwayTeamId);
                    int aGoals = 0, bGoals = 0;
                    int aAwayGoals = 0, bAwayGoals = 0;

                    foreach (var f in gf)
                    {
                        // identify which team corresponds to teamA/teamB in this fixture
                        // add goals
                        if (f.HomeTeamId == teamA && f.AwayTeamId == teamB)
                        {
                            aGoals += f.HomeTeamGoals ?? 0;
                            bGoals += f.AwayTeamGoals ?? 0;
                            bAwayGoals += f.AwayTeamGoals ?? 0;
                        }
                        else if (f.HomeTeamId == teamB && f.AwayTeamId == teamA)
                        {
                            bGoals += f.HomeTeamGoals ?? 0;
                            aGoals += f.AwayTeamGoals ?? 0;
                            aAwayGoals += f.AwayTeamGoals ?? 0;
                        }
                        else
                        {
                            // handle possibility of duplicate or odd
                            aGoals += f.HomeTeamId == teamA ? (f.HomeTeamGoals ?? 0) : (f.AwayTeamGoals ?? 0);
                            bGoals += f.HomeTeamId == teamB ? (f.HomeTeamGoals ?? 0) : (f.AwayTeamGoals ?? 0);
                        }
                    }

                    if (aGoals > bGoals) winners.Add(teamA);
                    else if (bGoals > aGoals) winners.Add(teamB);
                    else
                    {
                        // tie: use away goals
                        if (aAwayGoals > bAwayGoals) winners.Add(teamA);
                        else if (bAwayGoals > aAwayGoals) winners.Add(teamB);
                        else
                        {
                            // still tied -> penalties -> random pick for now
                            winners.Add(_rng.Next(2) == 0 ? teamA : teamB);
                        }
                    }
                }
            }
            else
            {
                // single leg knockouts; winner of each fixture advances
                foreach (var f in fixtures)
                {
                    if ((f.HomeTeamGoals ?? 0) > (f.AwayTeamGoals ?? 0)) winners.Add(f.HomeTeamId);
                    else if ((f.HomeTeamGoals ?? 0) < (f.AwayTeamGoals ?? 0)) winners.Add(f.AwayTeamId);
                    else winners.Add(_rng.Next(2) == 0 ? f.HomeTeamId : f.AwayTeamId);
                }
            }

            // winners list may contain duplicates if fixtures include both legs separately — ensure unique
            winners = winners.Distinct().ToList();

            // now create next phase fixtures or finalize if last
            var cupId = phase.EuropeanCupId;
            var cup = await _context.Set<Models.EuropeanCup>().Include(c => c.Template).ThenInclude(t => t.PhaseTemplates).FirstAsync(c => c.Id == cupId, ct);
            // find the next phase template by order
            var orderedTemplates = cup.Template.PhaseTemplates.OrderBy(p => p.Order).ToList();
            var currentIdx = orderedTemplates.FindIndex(pt => pt.Id == phase.PhaseTemplateId);
            if (currentIdx < 0) throw new InvalidOperationException("Current phase template not found in template list.");
            if (currentIdx == orderedTemplates.Count - 1)
            {
                // last phase -> winners[0] is champion if only one winner
                if (winners.Count == 1)
                {
                    // recommend to add WinnerId to EuropeanCup model; here we log
                    _logger.LogInformation("Cup {CupId} winner is team {TeamId}", cupId, winners.First());
                    // optionally set cup.WinnerId = winners.First(); _db.Update(cup); await _db.SaveChangesAsync(ct);
                }
                return;
            }

            var nextTemplate = orderedTemplates[currentIdx + 1];
            // create next phase instance (if not exists)
            var nextPhase = await _context.Set<EuropeanCupPhase>().FirstOrDefaultAsync(p => p.EuropeanCupId == cupId && p.PhaseTemplateId == nextTemplate.Id, ct);
            if (nextPhase == null)
            {
                nextPhase = new EuropeanCupPhase { EuropeanCupId = cupId, PhaseTemplateId = nextTemplate.Id };
                _context.Add(nextPhase);
                await _context.SaveChangesAsync(ct);
            }

            // Generate fixtures in next phase using winners (random pairings)
            var winnersShuf = winners.OrderBy(_ => _rng.Next()).ToList();
            var newFixtures = new List<Models.Fixture>();
            for (int i = 0; i < winnersShuf.Count; i += 2)
            {
                if (i + 1 >= winnersShuf.Count) break; // odd leftover (shouldn't happen in standard bracket)
                int a = winnersShuf[i];
                int b = winnersShuf[i + 1];
                if (nextTemplate.IsTwoLegged)
                {
                    newFixtures.Add(new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = nextPhase.Id,
                        HomeTeamId = a,
                        AwayTeamId = b,
                        Date = DateTime.UtcNow.AddDays(7 + i),
                        Round = 1,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = cup.GameSaveId
                    });
                    newFixtures.Add(new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = nextPhase.Id,
                        HomeTeamId = b,
                        AwayTeamId = a,
                        Date = DateTime.UtcNow.AddDays(14 + i),
                        Round = 2,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = cup.GameSaveId
                    });
                }
                else
                {
                    newFixtures.Add(new Models.Fixture
                    {
                        CompetitionType = CompetitionType.EuropeanCup,
                        EuropeanCupPhaseId = nextPhase.Id,
                        HomeTeamId = a,
                        AwayTeamId = b,
                        Date = DateTime.UtcNow.AddDays(7 + i),
                        Round = 1,
                        Status = MatchStatus.Scheduled,
                        GameSaveId = cup.GameSaveId
                    });
                }
            }

            if (newFixtures.Any())
            {
                await _context.AddRangeAsync(newFixtures, ct);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<Models.EuropeanCup?> GetByIdAsync(int europeanCupId, CancellationToken ct = default)
        {
            return await _context.Set<Models.EuropeanCup>()
                            .Include(c => c.Teams).ThenInclude(et => et.Team)
                            .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate)
                            .Include(c => c.Standings)
                            .FirstOrDefaultAsync(c => c.Id == europeanCupId, ct);
        }

        public Task FinalizeTournamentAsync(int europeanCupId, CancellationToken ct = default)
        {
            // optional: set WinnerId on EuropeanCup model (add property) and persist
            // additional cleanup, payouts, points, achievements, etc.
            _logger.LogInformation("FinalizeTournament called for {CupId}", europeanCupId);
            return Task.CompletedTask;
        }
    }
}