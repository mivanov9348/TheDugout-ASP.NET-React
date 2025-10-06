using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EuropeanCupController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public EuropeanCupController(DugoutDbContext context)
        {
            _context = context;
        }

        // GET: api/EuropeanCup/current?gameSaveId=5&seasonId=3
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentEuropeanCup(int gameSaveId, int seasonId)
        {
            var cup = await _context.Set<EuropeanCup>()
                .Include(c => c.Template)
                .Include(c => c.Teams).ThenInclude(ct => ct.Team)
                .Include(c => c.Standings).ThenInclude(s => s.Team)
                .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(c => c.GameSaveId == gameSaveId && c.SeasonId == seasonId);

            if (cup == null)
                return Ok(new { exists = false });

            // 🔹 взимаме id-тата на фазите
            var phaseIds = _context.EuropeanCupPhases.Select(p => p.Id).ToList();

            // 🔹 търсим фикстурите директно в context.Fixtures
            var fixtures = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Where(f => f.EuropeanCupPhaseId.HasValue && phaseIds.Contains(f.EuropeanCupPhaseId.Value))
                .ToListAsync();

            // 🔹 групови фази (само Group Stage)
            var groupPhase = await _context.EuropeanCupPhases
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.HomeTeam)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.AwayTeam)
                .FirstOrDefaultAsync(p => p.EuropeanCupId == cup.Id && !p.PhaseTemplate.IsKnockout);

            var groupFixtures = groupPhase != null
                ? groupPhase.Fixtures
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
                    })
                : Enumerable.Empty<object>();

            // 🔹 елиминации (всички knockout)
            var knockoutPhases = await _context.EuropeanCupPhases
                .Include(p => p.PhaseTemplate)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.HomeTeam)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.AwayTeam)
                .Where(p => p.EuropeanCupId == cup.Id && p.PhaseTemplate.IsKnockout)
                .OrderBy(p => p.PhaseTemplate.Order)
                .ToListAsync();

            var knockoutFixtures = knockoutPhases.Select(p => new
            {
                round = p.Id,
                name = p.PhaseTemplate.Name,
                matches = p.Fixtures.Select(f => new
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


            var result = new
            {
                exists = true,
                id = cup.Id,
                name = cup.Template.Name,
                logoFileName = cup.LogoFileName,
                teams = cup.Teams.Select(t => new
                {
                    id = t.Team.Id,
                    name = t.Team.Name,
                    abbreviation = t.Team.Abbreviation,
                    logoFileName = t.Team.LogoFileName
                }),
                standings = cup.Standings
                    .OrderByDescending(s => s.Points)
                    .ThenByDescending(s => s.GoalDifference)
                    .ThenByDescending(s => s.GoalsFor)
                    .Select(s => new
                    {
                        teamId = s.TeamId,
                        name = s.Team.Name,
                        logoFileName = s.Team.LogoFileName,
                        points = s.Points,
                        matches = s.Matches,
                        wins = s.Wins,
                        draws = s.Draws,
                        losses = s.Losses,
                        goalsFor = s.GoalsFor,
                        goalsAgainst = s.GoalsAgainst,
                        goalDifference = s.GoalDifference,
                        ranking = s.Ranking
                    }),
                fixtures = fixtures
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
                    }),
                groupFixtures = groupFixtures,
                knockoutFixtures = knockoutFixtures
            };

            return Ok(result);
        }


    }
}