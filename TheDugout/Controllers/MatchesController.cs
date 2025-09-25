using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Matches;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public class MatchesController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public MatchesController(DugoutDbContext context)
        {
            _context = context;
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

            return Ok(new
            {
                Matches = matches,
                UserTeamId = save.UserTeamId
            });
        }

        [Authorize]
        [HttpGet("{fixtureId}/preview")]
        public async Task<IActionResult> GetMatchPreview(int fixtureId)
        {
            try
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

                var result = new
                {
                    Competition = $"{competitionName} - Round {fixture.Round}",
                    Home = new
                    {
                        Name = fixture.HomeTeam?.Name ?? "Unknown",
                        Score = fixture.HomeTeamGoals ?? 0,
                        Lineup = fixture.HomeTeam?.Players.Select(p => new
                        {
                            Number = p.KitNumber,
                            Name = $"{p.FirstName} {p.LastName}",
                            Position = p.Position?.Code ?? "N/A"
                        })
                    },
                    Away = new
                    {
                        Name = fixture.AwayTeam?.Name ?? "Unknown",
                        Score = fixture.AwayTeamGoals ?? 0,
                        Lineup = fixture.AwayTeam?.Players.Select(p => new
                        {
                            Number = p.KitNumber,
                            Name = $"{p.FirstName} {p.LastName}",
                            Position = p.Position?.Code ?? "N/A"
                        })
                    },
                    Minute = fixture.Status == FixtureStatus.Played ? 90 : 0,
                    Status = fixture.Status.ToString()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{fixtureId}/match")]
        public async Task<IActionResult> Match(int fixtureId)
        {
            var fixture = await _context.Fixtures
                .Include(f => f.HomeTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
                .Include(f => f.AwayTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
                .FirstOrDefaultAsync(f => f.Id == fixtureId);

            if (fixture == null) return NotFound();

            var result = new
            {
                FixtureId = fixture.Id,
                HomeTeam = new
                {
                    Name = fixture.HomeTeam.Name,
                    Players = fixture.HomeTeam.Players.Select(p => new
                    {
                        Number = p.KitNumber,
                        Position = p.Position != null ? p.Position.Code : "N/A",
                        Name = $"{p.FirstName} {p.LastName}",
                        Stats = new
                        {
                            Goals = 0,   
                            Passes = 0
                        }
                    })
                },
                AwayTeam = new
                {
                    Name = fixture.AwayTeam.Name,
                    Players = fixture.AwayTeam.Players.Select(p => new
                    {
                        Number = p.KitNumber,
                        Position = p.Position != null ? p.Position.Code : "N/A",
                        Name = $"{p.FirstName} {p.LastName}",
                        Stats = new
                        {
                            Goals = 0,
                            Passes = 0
                        }
                    })
                },
                Status = fixture.Status.ToString(),
                Score = new
                {
                    Home = fixture.HomeTeamGoals ?? 0,
                    Away = fixture.AwayTeamGoals ?? 0
                }
            };

            return Ok(result);
        }

    }
}
