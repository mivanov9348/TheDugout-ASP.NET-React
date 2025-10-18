namespace TheDugout.Services.EuropeanCup
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.Fixture;
    public class EuropeanCupService : IEuropeanCupService
    {
        private readonly DugoutDbContext _context;
        private readonly IEurocupFixturesService _eurocupFixturesService;
        private readonly IEuroCupTeamService _euroCupTeamService;
        private readonly ILogger<EuropeanCupService> _logger;
        private readonly Random _rng = new Random();
        public EuropeanCupService(DugoutDbContext context, IEurocupFixturesService eurocupFixturesService, IEuroCupTeamService euroCupTeamService, ILogger<EuropeanCupService> logger)
        {
            _context = context;
            _eurocupFixturesService = eurocupFixturesService;
            _euroCupTeamService = euroCupTeamService;
            _logger = logger;
        }

        public async Task<EuropeanCup> InitializeTournamentAsync(
    int templateId,
    int gameSaveId,
    int seasonId,
    CancellationToken ct = default)
        {
            var template = await _context.Set<EuropeanCupTemplate>()
                .Include(t => t.PhaseTemplates)
                .FirstOrDefaultAsync(t => t.Id == templateId, ct)
                ?? throw new InvalidOperationException($"Template {templateId} not found.");

            if (!template.IsActive)
                throw new InvalidOperationException($"Template {templateId} is not active.");

            var eligibleTeams = await _context.Set<Models.Teams.Team>()
                .Where(t => t.LeagueId == null && t.GameSaveId == gameSaveId)
                .ToListAsync(ct);

            if (eligibleTeams.Count < template.TeamsCount)
                throw new InvalidOperationException($"Not enough teams for {template.Name}");

            var chosenTeams = eligibleTeams
                .OrderBy(_ => _rng.Next())
                .Take(template.TeamsCount)
                .ToList();

            var competition = new Competition
            {
                Type = CompetitionTypeEnum.EuropeanCup,
                GameSaveId = gameSaveId,
                SeasonId = seasonId
            };

            var euroCup = new EuropeanCup
            {
                TemplateId = template.Id,
                GameSaveId = gameSaveId,
                SeasonId = seasonId,
                LogoFileName = $"{template.Name}.png",
                IsActive = template.IsActive,
                Competition = competition
            };

            _context.EuropeanCups.Add(euroCup);
            await _context.SaveChangesAsync(ct);

            // Добавяме отбори, standings и фази
            var rankedTeams = chosenTeams
                .OrderByDescending(t => t.Popularity)
                .ThenBy(t => t.Name)
                .ToList();

            foreach (var team in rankedTeams)
            {

                await _euroCupTeamService.CreateTeamsForCupAsync(euroCup, rankedTeams, ct);

                _context.Add(new EuropeanCupStanding
                {
                    EuropeanCupId = euroCup.Id,
                    TeamId = team.Id,
                    GameSaveId = gameSaveId,
                    Points = 0,
                    Ranking = rankedTeams.IndexOf(team) + 1
                });
            }

            foreach (var pt in template.PhaseTemplates.OrderBy(p => p.Order))
                _context.Add(new EuropeanCupPhase { EuropeanCupId = euroCup.Id, PhaseTemplateId = pt.Id, GameSaveId = gameSaveId });

            await _context.SaveChangesAsync(ct);

            await _eurocupFixturesService.GenerateEuropeanLeaguePhaseFixturesAsync(euroCup.Id, seasonId, ct);
            return euroCup;
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
            var fixtures = await _context.Set<Fixture>()
                                    .Where(f => f.EuropeanCupPhaseId == europeanCupPhaseId && f.Status == MatchStageEnum.Played)
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
        public async Task<bool> IsEuropeanCupFinishedAsync(int europeanCupId)
        {
            var euroCup = await _context.EuropeanCups
                .Include(c => c.Phases)
                    .ThenInclude(p => p.Fixtures)
                .Include(c => c.Teams)
                .FirstOrDefaultAsync(c => c.Id == europeanCupId);

            if (euroCup == null)
                throw new Exception("European Cup not found");

            // Check if the cup has already been marked as finished
            bool allPhasesFinished = euroCup.Phases?
                .SelectMany(p => p.Fixtures)
                .All(f => f.Status == MatchStageEnum.Played) ?? false;

            // Check if only one team is left (not eliminated)
            bool onlyOneTeamLeft = euroCup.Teams.Count(t => !t.IsEliminated) <= 1;

            // If all phases are finished or only one team is left, mark the cup as finished
            if ((allPhasesFinished || onlyOneTeamLeft) && !euroCup.IsFinished)
            {
                euroCup.IsFinished = true;
                euroCup.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return euroCup.IsFinished;
        }
        public async Task<List<CompetitionSeasonResult>> GenerateEuropeanCupResultsAsync(int seasonId)
        {
            var europeanCups = await _context.EuropeanCups
                .Include(e => e.Template)
                .Include(e => e.Phases)
                    .ThenInclude(p => p.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                .Include(e => e.Phases)
                    .ThenInclude(p => p.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                .Where(e => e.SeasonId == seasonId && e.IsFinished)
                .ToListAsync();

            var results = new List<CompetitionSeasonResult>();

            foreach (var euro in europeanCups)
            {
                // Намираме последната фаза (финала)
                var finalPhase = euro.Phases
                    .OrderByDescending(p => p.PhaseTemplate.Order)
                    .FirstOrDefault();

                if (finalPhase == null)
                    continue;

                // Взимаме финалния мач
                var finalMatch = finalPhase.Fixtures
                    .Where(f => f.Status == MatchStageEnum.Played)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                if (finalMatch == null)
                    continue;

                // Определяме шампиона и финалиста по WinnerTeamId
                int? championTeamId = finalMatch.WinnerTeamId;
                int? runnerUpTeamId = null;

                if (championTeamId == finalMatch.HomeTeamId)
                    runnerUpTeamId = finalMatch.AwayTeamId;
                else if (championTeamId == finalMatch.AwayTeamId)
                    runnerUpTeamId = finalMatch.HomeTeamId;

                // fallback, ако WinnerTeamId липсва, ползвай головете
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

                // Създаваме резултата
                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.EuropeanCup,
                    CompetitionId = euro.CompetitionId,
                    GameSaveId = euro.GameSaveId,
                    ChampionTeamId = championTeamId,
                    RunnerUpTeamId = runnerUpTeamId,
                    Notes = $"Еврокупа {euro.Template.Name} - Финал: {finalMatch.HomeTeam?.Name} {finalMatch.HomeTeamGoals}:{finalMatch.AwayTeamGoals} {finalMatch.AwayTeam?.Name}"
                };

                results.Add(result);
            }

            if (results.Any())
            {
                await _context.CompetitionSeasonResults.AddRangeAsync(results);
                await _context.SaveChangesAsync();
            }

            return results;
        }
        public async Task HandleFinalMatchCompletionAsync(int europeanCupId, Fixture finalMatch, CancellationToken ct = default)
        {
            if (finalMatch == null)
                throw new ArgumentNullException(nameof(finalMatch));

            int? loserTeamId = null;

            if (finalMatch.WinnerTeamId == finalMatch.HomeTeamId)
                loserTeamId = finalMatch.AwayTeamId;
            else if (finalMatch.WinnerTeamId == finalMatch.AwayTeamId)
                loserTeamId = finalMatch.HomeTeamId;

            // fallback, ако WinnerTeamId липсва (напр. няма penalties, но има резултат)
            if (loserTeamId == null)
            {
                if (finalMatch.HomeTeamGoals > finalMatch.AwayTeamGoals)
                    loserTeamId = finalMatch.AwayTeamId;
                else
                    loserTeamId = finalMatch.HomeTeamId;
            }

            if (loserTeamId != null)
            {
                await _euroCupTeamService.MarkTeamEliminatedAsync(europeanCupId, loserTeamId.Value, ct);
                _logger.LogInformation("Final loser team {TeamId} eliminated from European Cup {CupId}.", loserTeamId, europeanCupId);
            }

            await IsEuropeanCupFinishedAsync(europeanCupId);
        }


    }
}