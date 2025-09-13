using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheDugout.Data;
using TheDugout.Models.Seasons;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/calendar")]
    public class CalendarController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public CalendarController(DugoutDbContext context)
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

        // 🔹 GET api/calendar?gameSaveId=1
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCalendar([FromQuery] int gameSaveId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var save = await _context.GameSaves
                .Include(gs => gs.Seasons)
                    .ThenInclude(s => s.Events)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId && gs.UserId == userId);

            if (save == null) return NotFound("Game save not found");

            var seasons = save.Seasons
                .Select(season => new
                {
                    seasonId = season.Id,
                    startDate = season.StartDate,
                    endDate = season.EndDate,
                    currentDate = season.CurrentDate,
                    events = season.Events
                        .OrderBy(e => e.Date)
                        .Select(e => new
                        {
                            id = e.Id,
                            date = e.Date,
                            type = e.Type.ToString(),
                            description = e.Description
                        })
                });

            return Ok(seasons);
        }

        // 🔹 POST api/calendar/event
        [Authorize]
        [HttpPost("event")]
        public async Task<IActionResult> AddEvent([FromBody] SeasonEvent newEvent)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var season = await _context.Seasons
                .Include(s => s.GameSave)
                .FirstOrDefaultAsync(s => s.Id == newEvent.SeasonId);

            if (season == null || season.GameSave.UserId != userId)
                return NotFound("Season not found or unauthorized");

            var entity = new SeasonEvent
            {
                SeasonId = newEvent.SeasonId,
                Date = newEvent.Date,
                Type = newEvent.Type,
                Description = newEvent.Description
            };

            _context.SeasonEvents.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = entity.Id,
                seasonId = entity.SeasonId,
                date = entity.Date,
                type = entity.Type.ToString(),
                description = entity.Description
            });
        }
    }
}
