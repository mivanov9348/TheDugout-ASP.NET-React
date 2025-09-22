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
            .Include(gs => gs.Seasons)
                .ThenInclude(s => s.Fixtures)
                    .ThenInclude(f => f.CupRound)
                        .ThenInclude(cr => cr.Cup)
                            .ThenInclude(c => c.Template)
            .Include(gs => gs.Seasons)
            //    .ThenInclude(s => s.Fixtures)
            //        .ThenInclude(f => f.LeagueSeason)
            //            .ThenInclude(ls => ls.League)
            //.Include(gs => gs.Seasons)
            //    .ThenInclude(s => s.Fixtures)
            //        .ThenInclude(f => f.EuropeanRound)
            //            .ThenInclude(er => er.Competition)
            .FirstOrDefaultAsync(gs => gs.Id == gameSaveId && gs.UserId == userId);

        if (save == null) return NotFound("Game save not found");

        var seasons = save.Seasons.Select(season => new
        {
            seasonId = season.Id,
            startDate = season.StartDate,
            endDate = season.EndDate,
            currentDate = season.CurrentDate,
            events = season.Events
                .OrderBy(e => e.Date)
                .Select(e =>
                {
                    string description = e.Description;

                    switch (e.Type)
                    {
                        case SeasonEventType.CupMatch:
                            var cupFixtures = season.Fixtures
                                .Where(f => f.Date.Date == e.Date.Date && f.CompetitionType == CompetitionType.DomesticCup)
                                .ToList();
                            if (cupFixtures.Any())
                                description = string.Join(", ",
                                    cupFixtures.Select(f => f.CupRound!.Cup.Template.Name).Distinct());
                            break;

                        //case SeasonEventType.ChampionshipMatch:
                        //    var leagueFixtures = season.Fixtures
                        //        .Where(f => f.Date.Date == e.Date.Date && f.CompetitionType == CompetitionType.League)
                        //        .ToList();
                        //    if (leagueFixtures.Any())
                        //        description = string.Join(", ",
                        //            leagueFixtures.Select(f => f.LeagueSeason!.League.Name).Distinct());
                        //    break;

                        //case SeasonEventType.EuropeanMatch:
                        //    var euroFixtures = season.Fixtures
                        //        .Where(f => f.Date.Date == e.Date.Date && f.CompetitionType == CompetitionType.European)
                        //        .ToList();
                        //    if (euroFixtures.Any())
                        //        description = string.Join(", ",
                        //            euroFixtures.Select(f => f.EuropeanRound!.Competition.Name).Distinct());
                        //    break;

                        case SeasonEventType.TransferWindow:
                            description = "Transfer window";
                            break;

                        default:
                            if (string.IsNullOrWhiteSpace(description))
                                description = "Free day";
                            break;
                    }

                    return new
                    {
                        id = e.Id,
                        date = e.Date,
                        type = e.Type.ToString(),
                        description,
                        isOccupied = e.IsOccupied
                    };
                })
        });

        return Ok(seasons);
    }


}
