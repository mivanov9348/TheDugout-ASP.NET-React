using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheDugout.Data;
using TheDugout.Models.Matches;
using TheDugout.Models.Seasons;

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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCalendar([FromQuery] int gameSaveId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var season = await _context.Seasons
            .Where(s => s.GameSaveId == gameSaveId && s.GameSave.UserId == userId)
            .Select(s => new
            {
                seasonId = s.Id,
                startDate = s.StartDate,
                endDate = s.EndDate,
                currentDate = s.CurrentDate,
                userTeamId = s.GameSave.UserTeamId,
                fixtures = s.Fixtures
                    .Select(f => new
                    {
                        f.Id,
                        f.Date,
                        f.CompetitionType,
                        HomeTeam = f.HomeTeam.Name,
                        AwayTeam = f.AwayTeam.Name,
                        f.HomeTeamId,
                        f.AwayTeamId
                    })
                    .ToList(),
                seasonEvents = s.Events // 🟢 взимаме и съществуващи евенти (transfer windows и т.н.)
                    .Select(e => new
                    {
                        e.Id,
                        e.Date,
                        e.Type,
                        e.Description,
                        e.IsOccupied
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (season == null)
            return NotFound("Game save not found");

        if (season.userTeamId == null)
            return BadRequest("User team not set for this save");

        // 🟢 филтрираме само мачовете на потребителския отбор
        var myFixtures = season.fixtures
            .Where(f => f.HomeTeamId == season.userTeamId || f.AwayTeamId == season.userTeamId)
            .OrderBy(f => f.Date)
            .ToList();

        var fixtureEvents = myFixtures.Select(f =>
        {
            bool isHome = f.HomeTeamId == season.userTeamId;
            string opponent = isHome ? f.AwayTeam : f.HomeTeam;
            string ha = isHome ? "(H)" : "(A)";

            string competition = f.CompetitionType.ToString();
            string description = $"{competition}, {opponent} {ha}";

            SeasonEventType type = f.CompetitionType switch
            {
                CompetitionType.League => SeasonEventType.ChampionshipMatch,
                CompetitionType.DomesticCup => SeasonEventType.CupMatch,
                CompetitionType.EuropeanCup => SeasonEventType.EuropeanMatch,
                _ => SeasonEventType.Other
            };

            return new
            {
                id = f.Id,
                date = f.Date,
                type = type.ToString(),
                description,
                isOccupied = true
            };
        }).ToList();

        // 🟢 други евенти от базата (напр. TransferWindow, Board meeting и т.н.)
        var otherEvents = season.seasonEvents.Select(e => new
        {
            id = e.Id,
            date = e.Date,
            type = e.Type.ToString(),
            description = string.IsNullOrWhiteSpace(e.Description) ? e.Type.ToString() : e.Description,
            isOccupied = e.IsOccupied
        }).ToList();

        // 🟢 комбинираме ги
        var allEvents = fixtureEvents.Concat(otherEvents).ToList();

        // 🟢 слагаме "Free day" за празните
        var allDates = Enumerable.Range(0, (season.endDate - season.startDate).Days + 1)
            .Select(offset => season.startDate.AddDays(offset).Date);

        var occupiedDates = allEvents.Select(e => e.date.Date).ToHashSet();

        var freeDayEvents = allDates
            .Where(d => !occupiedDates.Contains(d))
            .Select(d => new
            {
                id = 0,
                date = d,
                type = SeasonEventType.Other.ToString(),
                description = "Free day",
                isOccupied = false
            });

        var result = new
        {
            season.seasonId,
            season.startDate,
            season.endDate,
            season.currentDate,
            events = allEvents.Concat(freeDayEvents).OrderBy(e => e.date)
        };

        return Ok(result);
    }



}
