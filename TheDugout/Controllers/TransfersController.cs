using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public TransfersController(DugoutDbContext context)
        {
            _context = context;
        }

        [HttpGet("players")]
        public async Task<IActionResult> GetPlayers(
    int gameSaveId,
    string? search = null,
    string? team = null,
    string? country = null,
    string? position = null,
    bool freeAgent = false,
    string sortBy = "name",
    string sortOrder = "asc",
    int page = 1,
    int pageSize = 15)
        {
            var query = _context.Players
                .AsNoTracking()
                .Include(p => p.Team)
                .Include(p => p.Position)
                .Include(p => p.Country)
                .Where(p => p.GameSaveId == gameSaveId)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.ToLower();
                query = query.Where(p => p.FirstName.ToLower().Contains(lowered) || p.LastName.ToLower().Contains(lowered));
            }

            // Team filter
            if (!string.IsNullOrWhiteSpace(team))
                query = query.Where(p => p.Team != null && p.Team.Name.ToLower().Contains(team.ToLower()));

            // Country filter
            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(p => p.Country != null && p.Country.Name.ToLower().Contains(country.ToLower()));

            // Position filter
            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(p => p.Position != null && p.Position.Name.ToLower().Contains(position.ToLower()));

            // Free agent filter
            if (freeAgent)
                query = query.Where(p => p.TeamId == null);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "team" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Team != null).ThenByDescending(p => p.Team.Name)
                    : query.OrderBy(p => p.Team != null).ThenBy(p => p.Team.Name),
                "position" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Position.Name)
                    : query.OrderBy(p => p.Position.Name),
                "country" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Country.Name)
                    : query.OrderBy(p => p.Country.Name),
                "age" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Age)
                    : query.OrderBy(p => p.Age),
                "price" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "freeagent" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.TeamId == null)
                    : query.OrderBy(p => p.TeamId == null),
                _ => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.LastName)
                    : query.OrderBy(p => p.LastName),
            };

            var totalCount = await query.CountAsync();

            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    Name = p.FirstName + " " + p.LastName,
                    Team = p.Team != null ? p.Team.Name : "",
                    Country = p.Country != null ? p.Country.Name : "",
                    Position = p.Position != null ? p.Position.Name : "",
                    p.Age,
                    p.Price
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Players = players
            });
        }



        [HttpGet("attributes")]
        public async Task<IActionResult> GetAttributes(int gameSaveId)
        {
            var attributes = await _context.PlayerAttributes
                .Include(a => a.Player)
                .Include(a => a.Attribute)
                .Where(a => a.Player.GameSaveId == gameSaveId) // ✅ достъп през Player
                .Select(a => new
                {
                    a.Id,
                    a.Attribute.Code,
                    a.Attribute.Name,
                    a.Value,
                    PlayerId = a.PlayerId
                })
                .ToListAsync();

            return Ok(attributes);
        }
    }
}
