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
    [FromQuery] int? round = null // 🔹 опционален филтър
)
        {
            var query = _context.Fixtures
                .Where(f => f.GameSaveId == gameSaveId && f.SeasonId == seasonId);

            if (round.HasValue)
                query = query.Where(f => f.Round == round.Value);

            var grouped = await query
                .OrderBy(f => f.League.Template.Name)
                .ThenBy(f => f.Round)
                .ThenBy(f => f.Date)
                .Select(f => new
                {
                    f.LeagueId,
                    LeagueName = f.League.Template.Name,
                    f.Round,
                    Match = new
                    {
                        f.Id,
                        f.Date,
                        HomeTeam = f.HomeTeam.Name,
                        AwayTeam = f.AwayTeam.Name
                    }
                })
                .ToListAsync();

            var result = grouped
                .GroupBy(g => new { g.LeagueId, g.LeagueName, g.Round })
                .Select(g => new
                {
                    LeagueId = g.Key.LeagueId,
                    LeagueName = g.Key.LeagueName,
                    Round = g.Key.Round,
                    Matches = g.Select(x => x.Match).ToList()
                })
                .ToList();

            return Ok(result);
        }

    }
}
