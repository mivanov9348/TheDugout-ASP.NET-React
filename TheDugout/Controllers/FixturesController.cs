using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/fixtures")]
    public class FixturesController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public FixturesController(DugoutDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("{gameSaveId}/{seasonId}")]
        public async Task<IActionResult> GetFixtures(
    int gameSaveId,
    int seasonId,
    [FromQuery] int? round = 1,
    [FromQuery] int? leagueId = null
)
        {
            var query = _context.Fixtures
                .Where(f => f.GameSaveId == gameSaveId && f.SeasonId == seasonId);

            if (round.HasValue)
                query = query.Where(f => f.Round == round.Value);

            if (leagueId.HasValue)
            {
                query = query.Where(f => f.LeagueId == leagueId.Value);
            }
            else
            {
                var firstLeagueId = await _context.Leagues
                    .Where(l => l.GameSaveId == gameSaveId)
                    .OrderBy(l => l.Tier)
                    .Select(l => l.Id)
                    .FirstOrDefaultAsync();

                if (firstLeagueId != 0)
                    query = query.Where(f => f.LeagueId == firstLeagueId);
            }

            var fixtures = await query
                .OrderBy(f => f.Date)
                .Select(f => new
                {
                    f.LeagueId,
                    LeagueName = f.League.Template.Name,
                    f.Round,
                    f.Id,
                    f.Date,
                    HomeTeam = f.HomeTeam.Name,
                    AwayTeam = f.AwayTeam.Name,
                    f.HomeTeamGoals,
                    f.AwayTeamGoals,
                    HomeLogoFileName = f.HomeTeam.LogoFileName,   // 👈 добавено
                    AwayLogoFileName = f.AwayTeam.LogoFileName    // 👈 добавено
                })
                .ToListAsync();

            return Ok(fixtures);
        }



    }
}
