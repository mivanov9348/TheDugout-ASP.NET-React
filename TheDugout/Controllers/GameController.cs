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

        // GET api/games/saves
        [Authorize]
        [HttpGet("saves")]
        public async Task<IActionResult> GetUserGameSaves()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized(new { message = "User id not found in token" });

            var saves = await _context.GameSaves
                .AsNoTracking()
                .Where(gs => gs.UserId == userId.Value)
                .OrderByDescending(gs => gs.CreatedAt) // newest first
                .Select(gs => new
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    CreatedAt = gs.CreatedAt
                })
                .ToListAsync();

            return Ok(saves);
        }

        // DELETE api/games/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGameSave(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized(new { message = "User id not found in token" });

            var gameSave = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.Teams)
                .Include(gs => gs.Players)
                .Include(gs => gs.Leagues)
                .Include(gs => gs.Messages)
                .FirstOrDefaultAsync(gs => gs.Id == id && gs.UserId == userId.Value);

            if (gameSave == null)
                return NotFound(new { message = "Save not found or not owned by user" });

            _context.GameSaves.Remove(gameSave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game save deleted successfully" });
        }

        // POST api/games/new
        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> StartNewGame([FromBody] NewGameRequest req)
        {
            _logger.LogInformation("Claims in User: {Claims}",
                string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized(new { message = "User id not found in token" });

            if (req == null)
            {
                return BadRequest(new { message = "Invalid request body" });
            }

            var saveCount = await _context.GameSaves.CountAsync(gs => gs.UserId == userId.Value);
            if (saveCount >= 3)
            {
                return BadRequest(new { message = "3 saves Max!" });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var gameSave = new GameSave
                {
                    UserId = userId.Value,
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

                // Връщаме полета с имена, които фронтенда лесно ще използва:
                var resp = new
                {
                    id = gameSave.Id,
                    name = gameSave.Name,
                    createdAt = gameSave.CreatedAt,
                    seasonId = season.Id,
                    seasonStart = season.StartDate,
                    seasonEnd = season.EndDate
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


        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value   // fallback for JWT sub
                              ?? User.FindFirst("id")?.Value;   // extra fallback

            if (int.TryParse(userIdClaim, out var parsed)) return parsed;
            return null;
        }

    }
}