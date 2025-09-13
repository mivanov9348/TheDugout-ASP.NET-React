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

        // GET api/leagues?gameSaveId=1&seasonId=5 (опционално)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetLeagues([FromQuery] int gameSaveId, [FromQuery] int? seasonId = null)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            // Намираме твоя отбор (който е в този save)
            var myTeam = await _context.Teams
                .Include(t => t.League)
                    .ThenInclude(l => l.GameSave)
                .FirstOrDefaultAsync(t => t.GameSaveId == gameSaveId);

            if (myTeam == null) return NotFound("No team found for this save.");

            // Намираме текущия сезон (ако не е подаден — вземаме последния)
            var season = seasonId.HasValue
                ? await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId && s.GameSaveId == gameSaveId)
                : await _context.Seasons
                    .Where(s => s.GameSaveId == gameSaveId)
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync();

            if (season == null) return NotFound("No active season found.");

            // Вземаме всички лиги в този save + техните класирания за текущия сезон
            var leagues = await _context.Leagues
                .Include(l => l.Template)
                .Include(l => l.Teams)
                .Where(l => l.GameSaveId == gameSaveId)
                .Select(l => new
                {
                    id = l.Id,
                    name = l.Template.Name,
                    tier = l.Tier,
                    countryId = l.CountryId,
                    hasMyTeam = l.Teams.Any(t => t.Id == myTeam.Id),
                    teams = _context.LeagueStandings
                        .Where(ls => ls.LeagueId == l.Id && ls.SeasonId == season.Id)
                        .OrderBy(ls => ls.Ranking) // Сортиране по ранкинг
                        .Select(ls => new
                        {
                            id = ls.Team.Id,
                            name = ls.Team.Name,
                            abbreviation = ls.Team.Abbreviation,
                            logoFileName = ls.Team.LogoFileName,
                            matches = ls.Matches,
                            wins = ls.Wins,
                            draws = ls.Draws,
                            losses = ls.Losses,
                            goalsFor = ls.GoalsFor,
                            goalsAgainst = ls.GoalsAgainst,
                            goalDifference = ls.GoalDifference,
                            points = ls.Points,
                            ranking = ls.Ranking // 👈 КЛЮЧОВО: ДОБАВЕНО!
                        })
                        .ToList()
                })
                .ToListAsync();

            // Подреждаме: Първо — лигата на моя отбор, после останалите
            var ordered = leagues
                .OrderByDescending(l => l.hasMyTeam) // Моята лига е първа!
                .ToList();

            return Ok(ordered);
        }

        // GET api/leagues/seasons?gameSaveId=1 — за избор на сезон (опционално)
        [Authorize]
        [HttpGet("seasons")]
        public async Task<IActionResult> GetSeasons([FromQuery] int gameSaveId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var seasons = await _context.Seasons
                .Where(s => s.GameSaveId == gameSaveId)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new
                {
                    s.Id,
                    s.StartDate,
                    s.EndDate
                })
                .ToListAsync();

            return Ok(seasons);
        }
    }
}