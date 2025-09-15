using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheDugout.Data;

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
                        date = e.Date, // ISO string ще се върне
                        type = e.Type.ToString(),
                        description = e.Description,
                        isOccupied = e.IsOccupied
                    })
            });

        return Ok(seasons);
    }
}
