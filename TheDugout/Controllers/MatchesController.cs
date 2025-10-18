namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Services.Match.Interfaces;
    using TheDugout.Services.Player.Interfaces;

    [ApiController]
    [Route("api/matches")]
    public class MatchesController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly IMatchService _matchService;
        private readonly IMatchEngine _matchEngine;
        private readonly IPlayerStatsService _playerStatsService;
        private readonly IMatchResponseService _matchResponseService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(
            DugoutDbContext context,
            IMatchService matchService,
            IMatchEngine matchEngine,
            IPlayerStatsService playerStatsService,
            IMatchResponseService matchResponseService,
            ILogger<MatchesController> logger)
        {
            _context = context;
            _matchService = matchService;
            _matchEngine = matchEngine;
            _playerStatsService = playerStatsService;
            _matchResponseService = matchResponseService;
            _logger = logger;
        }

        [HttpGet("today/{gameSaveId}")]
        public async Task<IActionResult> GetTodayMatches(int gameSaveId)
        {
            var save = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.Fixtures)
                    .ThenInclude(f => f.Match)
                        .ThenInclude(m => m.Penalties)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (save == null) return NotFound();

            var today = save.Seasons.First().CurrentDate.Date;

            var fixtures = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.League).ThenInclude(l => l.Template)
                .Include(f => f.CupRound).ThenInclude(cr => cr.Cup).ThenInclude(c => c.Template)
                .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.EuropeanCup).ThenInclude(ec => ec.Template)
                .Include(f => f.Match).ThenInclude(m => m.Penalties)
                .Where(f => f.GameSaveId == gameSaveId && f.Date.Date == today)
                .ToListAsync();

            var matches = await _matchResponseService.GetFormattedMatchesResponseAsync(fixtures, save);

            return Ok(new { matches });
        }

        [HttpPost("simulate/{gameSaveId}")]
        public async Task<IActionResult> SimulateMatches(int gameSaveId)
        {
            var gameSave = await _context.GameSaves
                .Include(gs => gs.Fixtures)
                    .ThenInclude(f => f.Match)
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (gameSave == null)
                return NotFound($"GameSave {gameSaveId} not found.");

            var currentSeason = gameSave.Seasons
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            if (currentSeason == null)
                return BadRequest("No active season found for this GameSave.");

            var currentDate = currentSeason.CurrentDate.Date;
            var fixtures = gameSave.Fixtures
                .Where(f => f.Date.Date == currentDate)
                .ToList();

            if (!fixtures.Any())
                return Ok(new { message = "No fixtures to simulate." });

            foreach (var fixture in fixtures)
            {
                var match = fixture.Match;

                if (match == null)
                {
                    match = await _matchService.GetOrCreateMatchAsync(fixture, gameSave);
                    fixture.Match = match;
                }

                await _matchEngine.SimulateMatchAsync(fixture, gameSave);

                match.Status = MatchStageEnum.Played;
                fixture.Status = MatchStageEnum.Played;
            }

            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            var updatedSave = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(ut => ut.League)
                        .ThenInclude(l => l.Template)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            var updatedSeason = updatedSave.Seasons?.FirstOrDefault();
            var today = updatedSeason?.CurrentDate.Date ?? DateTime.Today;

            _context.ChangeTracker.Clear();

            var todayMatches = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.League).ThenInclude(l => l.Template)
                .Include(f => f.CupRound).ThenInclude(cr => cr.Cup).ThenInclude(c => c.Template)
                .Include(f => f.EuropeanCupPhase).ThenInclude(ecp => ecp.EuropeanCup).ThenInclude(ec => ec.Template)
                .Include(f => f.Match).ThenInclude(m => m.Penalties)
                .Where(f => f.GameSaveId == gameSaveId && f.Date.Date == today)
                .ToListAsync();

            var hasUnplayedMatchesToday = todayMatches.Any(m => m.Status == 0);
            var hasMatchesToday = todayMatches.Any();

            var matches = await _matchResponseService.GetFormattedMatchesResponseAsync(todayMatches, updatedSave);

            return Ok(new
            {
                message = "Matches simulated successfully",
                gameStatus = new
                {
                    gameSave = new
                    {
                        updatedSave.Id,
                        updatedSave.UserTeamId,
                        UserTeam = new
                        {
                            updatedSave.UserTeam.Name,
                            updatedSave.UserTeam.Balance,
                            LeagueName = updatedSave.UserTeam.League.Template.Name
                        },
                        Seasons = updatedSave.Seasons.Select(s => new
                        {
                            s.Id,
                            s.CurrentDate
                        }),
                    },
                    hasUnplayedMatchesToday,
                    hasMatchesToday
                },
                matches
            });
        }

        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatchDetails(int matchId)
        {
            _logger.LogInformation("==== GetMatchDetails called with matchId={MatchId} ====", matchId);

            var match = await _context.Matches
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                .Include(m => m.PlayerStats)
                    .ThenInclude(ps => ps.Player)
                .Include(m => m.Events)
                    .ThenInclude(e => e.EventType)
                .Include(m => m.Events)
                    .ThenInclude(e => e.Player)
                .Include(m => m.Competition)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                _logger.LogWarning("Match {MatchId} not found", matchId);
                return NotFound();
            }

            var fixture = match.Fixture;
            if (fixture == null)
            {
                _logger.LogWarning("Match {MatchId} has no fixture", matchId);
                return BadRequest("Match has no fixture assigned.");
            }

            // 🔹 Голмайстори — от PlayerMatchStats
            var goalScorers = match.PlayerStats
                .Where(ps => ps.Goals > 0)
                .Select(ps => new
                {
                    TeamId = ps.Player.TeamId,
                    PlayerId = ps.PlayerId,
                    Scorer = $"{ps.Player.FirstName} {ps.Player.LastName}",
                    Goals = ps.Goals
                })
                .ToList();

            // 🔹 Минутите — от MatchEvent (EventType = GOAL)
            var goalEvents = match.Events
                .Where(e => e.EventType.Code.Equals("GOAL", StringComparison.OrdinalIgnoreCase)
                         || e.EventType.Code.Equals("G", StringComparison.OrdinalIgnoreCase))
                .Select(e => new
                {
                    e.TeamId,
                    e.PlayerId,
                    e.Minute
                })
                .ToList();

            // 🔹 Комбинираме данните: голмайстори + минута
            List<object> BuildGoals(int? teamId)
            {
                var teamGoals = new List<object>();

                foreach (var scorer in goalScorers.Where(s => s.TeamId == teamId))
                {
                    // 🔹 Списък от nullable int (int?) – защото може да няма минута
                    var minutes = goalEvents
                        .Where(g => g.TeamId == teamId && g.PlayerId == scorer.PlayerId)
                        .Select(g => (int?)g.Minute)
                        .OrderBy(m => m)
                        .ToList();

                    // 🔹 Ако няма минути – добавяме null за всеки гол
                    if (!minutes.Any())
                        minutes.AddRange(Enumerable.Repeat<int?>(null, scorer.Goals));

                    for (int i = 0; i < scorer.Goals; i++)
                    {
                        var minute = i < minutes.Count ? minutes[i] : (int?)null;
                        teamGoals.Add(new
                        {
                            minute,
                            scorer = scorer.Scorer,
                            playerId = scorer.PlayerId
                        });
                    }
                }

                // 🔹 Сортираме по минута, null в края
                return teamGoals.OrderBy(g => ((dynamic)g).minute ?? 999).ToList();
            }


            var homeGoals = BuildGoals(fixture.HomeTeamId);
            var awayGoals = BuildGoals(fixture.AwayTeamId);

            // 🔹 DTO за фронтенда
            var dto = new
            {
                id = match.Id,
                date = fixture.Date,
                //competition = match.Competition?.Name ?? "Unknown competition",
                status = match.Status.ToString(),
                currentMinute = match.CurrentMinute,
                result = $"{fixture.HomeTeamGoals ?? 0} - {fixture.AwayTeamGoals ?? 0}",
                winner = fixture.WinnerTeamId == fixture.HomeTeamId
                    ? fixture.HomeTeam?.Name
                    : fixture.WinnerTeamId == fixture.AwayTeamId
                        ? fixture.AwayTeam?.Name
                        : null,
                homeTeam = new
                {
                    name = fixture.HomeTeam?.Name ?? "Home Team",
                    logo = string.IsNullOrWhiteSpace(fixture.HomeTeam?.LogoFileName)
                        ? null
                        : $"/uploads/logos/{fixture.HomeTeam.LogoFileName}",
                    goals = homeGoals
                },
                awayTeam = new
                {
                    name = fixture.AwayTeam?.Name ?? "Away Team",
                    logo = string.IsNullOrWhiteSpace(fixture.AwayTeam?.LogoFileName)
                        ? null
                        : $"/uploads/logos/{fixture.AwayTeam.LogoFileName}",
                    goals = awayGoals
                }
            };

            _logger.LogInformation(
                "Returning match DTO with {HomeGoals}-{AwayGoals} goals",
                fixture.HomeTeamGoals, fixture.AwayTeamGoals
            );

            return Ok(dto);
        }


    }
}