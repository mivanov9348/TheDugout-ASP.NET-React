using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Services.EuropeanCup;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EuropeanCupController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly IEuropeanCupStandingService _standingService;
        private readonly IEurocupFixturesService _fixturesService;
        private readonly IEurocupKnockoutService _knockoutService;

        public EuropeanCupController(
            DugoutDbContext context,
            IEuropeanCupStandingService standingService,
            IEurocupFixturesService fixturesService,
            IEurocupKnockoutService knockoutService)
        {
            _context = context;
            _standingService = standingService;
            _fixturesService = fixturesService;
            _knockoutService = knockoutService;
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

            var allFixtures = await _fixturesService.GetAllFixturesForCupAsync(cup.Id);
            var groupFixtures = await _fixturesService.GetGroupFixturesAsync(cup.Id);
            var knockoutFixtures = await _knockoutService.GetKnockoutFixturesAsync(cup.Id);
            var sortedStandings = await _standingService.GetSortedStandingsAsync(cup.Id);

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
                standings = sortedStandings,
                fixtures = allFixtures
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