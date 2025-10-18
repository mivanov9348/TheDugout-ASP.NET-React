namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;

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
    [FromQuery] int? leagueId = null)
        {
            // Базов филтър – винаги по GameSaveId и Season
            var query = _context.Fixtures
                .Include(f => f.League).ThenInclude(l => l.Template)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Where(f => f.GameSaveId == gameSaveId && f.SeasonId == seasonId);

            // Филтрирай по кръг
            if (round.HasValue && round.Value > 0)
                query = query.Where(f => f.Round == round.Value);

            // Ако има избрана лига — филтрирай по нея
            if (leagueId.HasValue && leagueId.Value > 0)
            {
                query = query.Where(f => f.LeagueId == leagueId.Value);
            }
            // АКО НЯМА ИЗБРАНА ЛИГА - ВРЪЩАМЕ ВСИЧКИ МАЧОВЕ ЗА ТОЗИ SEASON
            // Не филтрираме автоматично по първата лига!

            var fixtures = await query
                .OrderBy(f => f.League.Tier)
                .ThenBy(f => f.Date)
                .Select(f => new
                {
                    f.Id,
                    f.GameSaveId,
                    f.SeasonId,
                    f.LeagueId,
                    LeagueName = f.League.Template.Name,
                    f.Round,
                    f.Date,
                    HomeTeam = f.HomeTeam != null ? f.HomeTeam.Name : "—",
                    AwayTeam = f.AwayTeam != null ? f.AwayTeam.Name : "—",
                    f.HomeTeamGoals,
                    f.AwayTeamGoals,
                    HomeLogoFileName = f.HomeTeam != null ? f.HomeTeam.LogoFileName : null,
                    AwayLogoFileName = f.AwayTeam != null ? f.AwayTeam.LogoFileName : null
                })
                .ToListAsync();

            return Ok(fixtures);
        }
    }
}
