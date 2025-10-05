using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Fixtures;
using TheDugout.Services.Fixture;


namespace TheDugout.Services.EuropeanCup
{
    public class EuropeanCupService : IEuropeanCupService
    {
        private readonly DugoutDbContext _context;
        //private readonly IFixturesService _fixturesService;
        private readonly IEurocupFixturesService _eurocupFixturesService;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly ILogger<EuropeanCupService> _logger;
        private readonly Random _rng = new Random();
        public EuropeanCupService(DugoutDbContext context, IFixturesHelperService fixturesHelperService, IEurocupFixturesService eurocupFixturesService, ILogger<EuropeanCupService> logger)
        {
            _context = context;
            _eurocupFixturesService = eurocupFixturesService;
            _fixturesHelperService = fixturesHelperService;
            _logger = logger;
        }

        public async Task<Models.Competitions.EuropeanCup> InitializeTournamentAsync(
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

            var teams = await _context.Set<Models.Teams.Team>().ToListAsync();

            // 2. Взимаме eligible отбори (LeagueId == null и същия GameSave)
            var eligibleTeams = await _context.Set<Models.Teams.Team>()
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

            string logoFileName = $"{template.Name}.png";

            string validLogoFileName = new string(logoFileName
                .Select(c => c switch
                {
                    '\\' or '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|' => '_',
                    _ => c
                })
                .ToArray());


            // 4. Създаваме Cup
            var cup = new Models.Competitions.EuropeanCup
            {
                TemplateId = template.Id,
                GameSaveId = gameSaveId,
                SeasonId = seasonId,
                LogoFileName = logoFileName
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
                    TeamId = team.Id,
                    CurrentPhaseOrder = 1,
                    IsEliminated = false,
                    IsPlayoffParticipant = false
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
            _logger.LogInformation("Initialized EuropeanCup {CupId} with {Teams} teams and logo: {Logo}", cup.Id, chosenTeams.Count, validLogoFileName);

            return cup;
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
            var fixtures = await _context.Set<Models.Fixtures.Fixture>()
                                    .Where(f => f.EuropeanCupPhaseId == europeanCupPhaseId && f.Status == FixtureStatus.Played)
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

    }
}