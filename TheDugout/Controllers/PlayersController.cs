using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheDugout.Data;
using TheDugout.Models;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public PlayersController(DugoutDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPlayers(
    [FromQuery] int gameSaveId,
    int? teamId,
    int? countryId,
    int? minAge,
    int? maxAge,
    int? positionId,
    string? sortBy = "Name",
    string? sortOrder = "asc")
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            // ✅ проверка за сейфа да е на този потребител
            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId && gs.UserId == userId);

            if (gameSave == null) return Forbid(); // сейфът не е негов

            var players = _context.Players
                .Where(p => p.GameSaveId == gameSaveId)
                .Include(p => p.Team)
                .Include(p => p.Country)
                .Include(p => p.Position)
                .Include(p => p.Attributes).ThenInclude(pa => pa.Attribute)
                .AsQueryable();

            if (teamId.HasValue)
                players = players.Where(p => p.TeamId == teamId.Value);

            if (countryId.HasValue)
                players = players.Where(p => p.CountryId == countryId.Value);

            if (positionId.HasValue)
                players = players.Where(p => p.PositionId == positionId.Value);

            if (minAge.HasValue)
                players = players.Where(p => p.Age >= minAge.Value);

            if (maxAge.HasValue)
                players = players.Where(p => p.Age <= maxAge.Value);

            // ✅ сортиране
            players = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("age", "desc") => players.OrderByDescending(p => p.Age),
                ("age", _) => players.OrderBy(p => p.Age),

                ("team", "desc") => players.OrderByDescending(p => p.Team.Name),
                ("team", _) => players.OrderBy(p => p.Team.Name),

                ("country", "desc") => players.OrderByDescending(p => p.Country!.Name),
                ("country", _) => players.OrderBy(p => p.Country!.Name),

                ("position", "desc") => players.OrderByDescending(p => p.Position.Name),
                ("position", _) => players.OrderBy(p => p.Position.Name),

                ("name", "desc") => players.OrderByDescending(p => p.LastName).ThenBy(p => p.FirstName),
                _ => players.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            };

            var result = await players.Select(p => new
            {
                p.Id,
                Name = p.FirstName + " " + p.LastName,
                Team = p.Team != null ? p.Team.Name : "-",
                Country = p.Country != null ? p.Country.Name : "-",
                p.Age,
                Position = p.Position != null ? p.Position.Name : "-",
                p.KitNumber,
                p.HeightCm,
                p.WeightKg,
                Attributes = p.Attributes.Select(a => new
                {
                    a.Attribute.Name,
                    a.Value
                })
            }).ToListAsync();

            return Ok(result);
        }


        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst("id")?.Value;

            if (int.TryParse(userIdClaim, out var parsed)) return parsed;
            return null;
        }


    }
}
