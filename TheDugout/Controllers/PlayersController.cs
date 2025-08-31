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

        // GET: api/players?gameSaveId=1
        [HttpGet]
        public async Task<IActionResult> GetPlayers(int gameSaveId)
        {
            var players = await _context.Players
                .Include(p => p.Team)
                .Include(p => p.Country)
                .Include(p => p.Position)
                .Include(p => p.Attributes)
                    .ThenInclude(pa => pa.Attribute)
                .Where(p => p.GameSaveId == gameSaveId)
                .Select(p => new
                {
                    p.Id,
                    Name = p.FirstName + " " + p.LastName,
                    Team = p.Team != null ? p.Team.Name : string.Empty,
                    Country = p.Country != null ? p.Country.Name : string.Empty,
                    Position = p.Position != null ? p.Position.Name : string.Empty,
                    p.Age,
                    Attributes = p.Attributes.Select(pa => new
                    {
                        pa.AttributeId,
                        pa.Attribute.Name,
                        pa.Value
                    }).ToList()
                })
                .ToListAsync();

            return Ok(players);
        }

        // GET: api/players/attributes?gameSaveId=1
        [HttpGet("attributes")]
        public async Task<IActionResult> GetAttributes(int gameSaveId)
        {
            var attributes = await _context.PlayerAttributes
                .Where(pa => pa.Player.GameSaveId == gameSaveId)
                .Select(pa => pa.Attribute)
                .Distinct()
                .ToListAsync();

            return Ok(attributes.Select(a => new
            {
                a.Id,
                a.Name,
                a.Code
            }));
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("id")?.Value;

            return int.TryParse(userIdClaim, out var parsed)
                ? parsed
                : null;
        }
    }
}
