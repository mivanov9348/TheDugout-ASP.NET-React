using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using TheDugout.Data;
using TheDugout.Data.DtoNewGame;
using TheDugout.Models;


namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/games")]

    public class GameController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameController> _logger;


        public GameController(DugoutDbContext context, ILogger<GameController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET api/games/teamtemplates
        [HttpGet("teamtemplates")]
        public async Task<IActionResult> GetTeamTemplates()
        {
            var list = await _context.TeamTemplates
                       .AsNoTracking()
                       .Include(t => t.League)
                       .Select(t => new TeamTemplateDto
                       {
                           Id = t.Id,
                           Name = t.Name,
                           Abbreviation = t.Abbreviation,
                           CountryId = t.CountryId,
                           LeagueId = t.LeagueId,
                           LeagueName = t.League.Name,
                           Tier = t.League.Tier
                       })
                       .ToListAsync();

            return Ok(list);
        }

        // POST api/games/new
        [HttpPost("new")]
        public async Task<IActionResult> StartNewGame([FromBody] NewGameRequest req)
        {
            // 1) Get userId from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User id not found in token" });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var gameSave = new GameSave
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                };

                var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
                var endDate = startDate.AddYears(1).AddDays(-1);
                var season = new Season
                {
                    GameSave = gameSave,
                    StartDate = startDate,
                    EndDate = endDate,
                    CurrentDate = startDate
                };
                gameSave.Seasons.Add(season);

                _context.GameSaves.Add(gameSave);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var resp = new NewGameResponse
                {
                    GameSaveId = gameSave.Id,
                    Name = gameSave.Name,
                    CreatedAt = gameSave.CreatedAt,
                    SeasonId = season.Id,
                    SeasonStart = season.StartDate,
                    SeasonEnd = season.EndDate,

                };
                return Ok(resp);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create minimal new game for user {UserId}", userId);
                return StatusCode(500, new { message = "Failed to create new game" });
            }

        }

        // Logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out successfully" });
        }
    }
}