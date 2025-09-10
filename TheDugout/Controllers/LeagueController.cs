using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using System.Security.Claims;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/leagues")]
    public class LeagueController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public LeagueController(DugoutDbContext context)
        {
            _context = context;
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst("id")?.Value;

            if (int.TryParse(userIdClaim, out var parsed)) return parsed;
            return null;
        }

        // GET api/leagues?gameSaveId=1
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetLeagues([FromQuery] int gameSaveId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var myTeam = await _context.Teams
        .FirstOrDefaultAsync(t => t.GameSaveId == gameSaveId);

            var leaguesQuery = _context.Leagues
        .Include(l => l.Teams)
        .Where(l => l.GameSaveId == gameSaveId);

            var leagues = await leaguesQuery
        .Select(l => new
        {
            id = l.Id,
            name = l.Template.Name,
            tier = l.Tier,
            countryId = l.CountryId,
            rounds = l.Teams
                .SelectMany(t => t.HomeFixtures)
                .Max(f => (int?)f.Round) ?? 0,
            hasMyTeam = l.Teams.Any(t => t.Id == myTeam.Id), 
            teams = l.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ThenByDescending(t => t.GoalsFor)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    abbreviation = t.Abbreviation,
                    points = t.Points,
                    wins = t.Wins,
                    draws = t.Draws,
                    losses = t.Losses,
                    goalsFor = t.GoalsFor,
                    goalsAgainst = t.GoalsAgainst,
                    goalDifference = t.GoalDifference,
                })
        })
        .ToListAsync();

            var ordered = leagues
        .OrderByDescending(l => l.hasMyTeam)
        .ToList();

            return Ok(ordered);
        }
    }
}
