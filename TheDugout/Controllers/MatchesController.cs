using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Matches;
using TheDugout.Services.Match;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly DugoutDbContext _context;
    private readonly IMatchService _matchService;

    public MatchesController(DugoutDbContext context, IMatchService matchService)
    {
        _context = context;
        _matchService = matchService;
    }

    [Authorize]
    [HttpGet("today/{gameSaveId}")]
    public async Task<IActionResult> GetTodayMatches(int gameSaveId)
    {
        var save = await _context.GameSaves
            .Include(gs => gs.Seasons)
            .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

        if (save == null) return NotFound();

        var today = save.Seasons.First().CurrentDate.Date;

        var matches = await _context.Fixtures
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .Include(f => f.League).ThenInclude(l => l.Template)
            .Include(f => f.CupRound).ThenInclude(cr => cr.Cup).ThenInclude(c => c.Template)
            .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.EuropeanCup).ThenInclude(ec => ec.Template)
            .Where(f => f.GameSaveId == gameSaveId && f.Date.Date == today)
            .Select(f => new
            {
                FixtureId = f.Id,
                CompetitionType = f.CompetitionType.ToString(),
                CompetitionName =
                    f.CompetitionType == CompetitionType.League
                        ? f.League!.Template.Name
                        : f.CompetitionType == CompetitionType.DomesticCup
                            ? f.CupRound!.Cup.Template.Name
                            : f.EuropeanCupPhase!.EuropeanCup.Template.Name,
                Home = f.HomeTeam.Name,
                Away = f.AwayTeam.Name,
                HomeTeamId = f.HomeTeam.Id,
                AwayTeamId = f.AwayTeam.Id,
                Time = f.Date.ToString("HH:mm"),
                IsUserTeamMatch = (f.HomeTeamId == save.UserTeamId || f.AwayTeamId == save.UserTeamId)
            })
            .ToListAsync();

        return Ok(new { Matches = matches, save.UserTeamId });
    }

    [Authorize]
    [HttpGet("{fixtureId}/preview")]
    public async Task<IActionResult> GetMatchPreview(int fixtureId)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.HomeTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
            .Include(f => f.AwayTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
            .Include(f => f.League).ThenInclude(l => l.Template)
            .Include(f => f.CupRound).ThenInclude(cr => cr.Cup).ThenInclude(c => c.Template)
            .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.EuropeanCup).ThenInclude(ec => ec.Template)
            .FirstOrDefaultAsync(f => f.Id == fixtureId);

        if (fixture == null)
            return NotFound(new { error = "Fixture not found" });

        var competitionName =
            fixture.CompetitionType == CompetitionType.League
                ? fixture.League?.Template.Name
                : fixture.CompetitionType == CompetitionType.DomesticCup
                    ? fixture.CupRound?.Cup?.Template.Name
                    : fixture.EuropeanCupPhase?.EuropeanCup?.Template.Name;

        return Ok(new
        {
            Competition = $"{competitionName} - Round {fixture.Round}",
            Home = new
            {
                fixture.HomeTeam!.Name,
                Score = fixture.HomeTeamGoals ?? 0,
                Lineup = fixture.HomeTeam.Players.Select(p => new
                {
                    p.KitNumber,
                    Name = $"{p.FirstName} {p.LastName}",
                    Position = p.Position?.Code ?? "N/A"
                })
            },
            Away = new
            {
                fixture.AwayTeam!.Name,
                Score = fixture.AwayTeamGoals ?? 0,
                Lineup = fixture.AwayTeam.Players.Select(p => new
                {
                    p.KitNumber,
                    Name = $"{p.FirstName} {p.LastName}",
                    Position = p.Position?.Code ?? "N/A"
                })
            },
            Minute = fixture.Status == FixtureStatus.Played ? 90 : 0,
            Status = fixture.Status.ToString()
        });
    }

    [Authorize]
    [HttpGet("{fixtureId}/match")]
    public async Task<IActionResult> StartOrGetMatch(int fixtureId)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.Matches)
            .FirstOrDefaultAsync(f => f.Id == fixtureId);

        if (fixture == null)
            return NotFound(new { error = "Fixture not found" });

        var match = fixture.Matches.OrderByDescending(m => m.Id).FirstOrDefault();
        if (match == null)
        {
            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(gs => gs.Id == fixture.GameSaveId);
            if (gameSave == null)
                return NotFound(new { error = "GameSave not found" });

            match = await _matchService.CreateMatchFromFixtureAsync(fixture, gameSave);
        }

        var view = await _matchService.GetMatchViewByIdAsync(match.Id);
        return Ok(view);
    }

    [Authorize]
    [HttpGet("{matchId}")]
    public async Task<IActionResult> GetMatch(int matchId)

    {
        var view = await _matchService.GetMatchViewByIdAsync(matchId);
        if (view == null)
            return NotFound(new { error = "Match not found" });

        return Ok(view);
    }
}
