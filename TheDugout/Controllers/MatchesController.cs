using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Matches;
using TheDugout.Services.Match;
using TheDugout.Services.MatchEngine;
using TheDugout.Services.Player;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly DugoutDbContext _context;
    private readonly IMatchService _matchService;
    private readonly IMatchEngine _matchEngine;
    private readonly IPlayerStatsService _playerStatsService;

    public MatchesController(DugoutDbContext context, IMatchService matchService, IMatchEngine matchEngine, IPlayerStatsService playerStatsService)
    {
        _context = context;
        _matchService = matchService;
        _matchEngine = matchEngine;
        _playerStatsService = playerStatsService;
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

        var events = await _context.MatchEvents
            .Where(e => e.MatchId == matchId)
            .OrderByDescending(e => e.Minute)
            .Select(e => new
            {
                e.Minute,   
                Text = e.Commentary,
                Team = e.Team.Name,
                Player = e.Player.FirstName + " " + e.Player.LastName,
                EventType = e.EventType.Name,
                Outcome = e.Outcome.Name
            })
            .ToListAsync();

        return Ok(new { view, events });
    }

    [Authorize]
    [HttpGet("active/{gameSaveId}")]
    public async Task<IActionResult> GetActiveMatch(int gameSaveId)
    {
        var match = await _context.Matches
            .Where(m => m.GameSaveId == gameSaveId && m.Status == MatchStatus.Live)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        if (match == null) return Ok(null);

        return Ok(new { match.Id, match.FixtureId });
    }

        [HttpPost("{id}/step")]
        public async Task<IActionResult> StepMatch(int id)
        {
            var match = await _context.Matches
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                        .ThenInclude(t => t.Players)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                        .ThenInclude(t => t.Players)
                .Include(m => m.PlayerStats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound();

            if (match.PlayerStats == null || !match.PlayerStats.Any())
            {
                var stats = _playerStatsService.InitializeMatchStats(match);
                match.PlayerStats = stats;
                _context.PlayerMatchStats.AddRange(stats);
                await _context.SaveChangesAsync();
            }

            var matchEvent = await _matchEngine.PlayStep(match);

            await _context.SaveChangesAsync();

            var events = await _context.MatchEvents
            .Where(e => e.MatchId == match.Id)
            .OrderByDescending(e => e.Minute)
            .ThenByDescending(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.Minute,
                Description = e.Commentary,
                TeamId = e.TeamId,
                Team = e.Team.Name,
                PlayerId = e.PlayerId,
                PlayerName = e.Player.FirstName + " " + e.Player.LastName,
                EventType = e.EventType.Name,
                Outcome = e.Outcome.Name
            })
            .ToListAsync();

            return Ok(new
            {
                finished = matchEvent == null,
                matchStatus = match.Status,
                minute = match.CurrentMinute,
                HomeScore = match.Fixture.HomeTeamGoals,
                AwayScore = match.Fixture.AwayTeamGoals,
                matchEvent = matchEvent == null ? null : new
                {
                    matchEvent.Id,
                    matchEvent.Minute,
                    Description = matchEvent.Commentary,
                    TeamId = matchEvent.TeamId,
                    PlayerId = matchEvent.PlayerId,
                    PlayerName = matchEvent.Player.FirstName + " " + matchEvent.Player.LastName,
                },
                events
            });

        }
}
