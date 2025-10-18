namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Match;
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

        public MatchesController(DugoutDbContext context, IMatchService matchService, IMatchEngine matchEngine, IPlayerStatsService playerStatsService)
        {
            _context = context;
            _matchService = matchService;
            _matchEngine = matchEngine;
            _playerStatsService = playerStatsService;
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

            var matches = fixtures
    .Select(f =>
    {
        var homePens = f.Match?.Penalties.Count(p => p.TeamId == f.HomeTeamId && p.IsScored) ?? 0;
        var awayPens = f.Match?.Penalties.Count(p => p.TeamId == f.AwayTeamId && p.IsScored) ?? 0;

        string? winner = null;

        if (f.HomeTeamGoals > f.AwayTeamGoals)
        {
            winner = f.HomeTeam?.Name;
        }
        else if (f.AwayTeamGoals > f.HomeTeamGoals)
        {
            winner = f.AwayTeam?.Name;
        }
        // Ако е елиминация и се стигне до дузпи
        else if (f.IsElimination && (homePens > 0 || awayPens > 0))
        {
            winner = homePens > awayPens ? f.HomeTeam?.Name : f.AwayTeam?.Name;
        }

        return new
        {
            FixtureId = f.Id,
            CompetitionName =
                f.CompetitionType == CompetitionTypeEnum.League
                    ? f.League!.Template.Name
                    : f.CompetitionType == CompetitionTypeEnum.DomesticCup
                        ? f.CupRound!.Cup.Template.Name
                        : f.EuropeanCupPhase!.EuropeanCup.Template.Name,
            Home = f.HomeTeam.Name,
            Away = f.AwayTeam.Name,
            HomeGoals = f.HomeTeamGoals,
            AwayGoals = f.AwayTeamGoals,
            Status = f.Match != null ? (int)f.Match.Status : (int)f.Status,
            IsUserTeamMatch = (f.HomeTeamId == save.UserTeamId || f.AwayTeamId == save.UserTeamId),
            HomePenalties = homePens,
            AwayPenalties = awayPens,
            Winner = winner,
            IsElimination = f.CompetitionType == CompetitionTypeEnum.DomesticCup
                         || f.CompetitionType == CompetitionTypeEnum.EuropeanCup
        };
    })
    .ToList();


            return Ok(new
            {
                matches               
            });
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

            // 👇 ВЗЕМИ АКТУАЛИЗИРАНАТА ИНФОРМАЦИЯ ЗА ХЕДЪРА
            var updatedSave = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(ut => ut.League)
                        .ThenInclude(l => l.Template)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            var updatedSeason = updatedSave.Seasons?.FirstOrDefault();
            var today = updatedSeason?.CurrentDate.Date ?? DateTime.Today;

            _context.ChangeTracker.Clear();

            // 👇 ДОБАВИ INCLUDE ЗА ВСИЧКИ NAVIGATION PROPERTIES
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

            var matches = todayMatches.Select(f =>
            {
                var homePens = f.Match?.Penalties.Count(p => p.TeamId == f.HomeTeamId && p.IsScored) ?? 0;
                var awayPens = f.Match?.Penalties.Count(p => p.TeamId == f.AwayTeamId && p.IsScored) ?? 0;


                bool isElimination = f.CompetitionType == CompetitionTypeEnum.DomesticCup
                                  || f.CompetitionType == CompetitionTypeEnum.EuropeanCup;

                string? winner = null;

                if (f.HomeTeamGoals > f.AwayTeamGoals)
                {
                    winner = f.HomeTeam?.Name;
                }
                else if (f.AwayTeamGoals > f.HomeTeamGoals)
                {
                    winner = f.AwayTeam?.Name;
                }
                else if (f.IsElimination && (homePens > 0 || awayPens > 0))
                {
                    winner = homePens > awayPens ? f.HomeTeam?.Name : f.AwayTeam?.Name;
                }

                return new
                {
                    FixtureId = f.Id,
                    CompetitionName = GetCompetitionName(f),
                    Home = f.HomeTeam?.Name ?? "Unknown Team",
                    Away = f.AwayTeam?.Name ?? "Unknown Team",
                    HomeGoals = f.HomeTeamGoals,
                    AwayGoals = f.AwayTeamGoals,
                    Status = f.Match != null ? (int)f.Match.Status : (int)f.Status,
                    IsUserTeamMatch = (f.HomeTeamId == updatedSave.UserTeamId || f.AwayTeamId == updatedSave.UserTeamId),
                    HomePenalties = homePens,
                    AwayPenalties = awayPens,
                    Winner = winner,
                    IsElimination = isElimination
                };
            }).ToList();


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

        private static string GetCompetitionName(Fixture fixture)
        {
            try
            {
                return fixture.CompetitionType switch
                {
                    CompetitionTypeEnum.League => fixture.League?.Template?.Name ?? "Unknown League",
                    CompetitionTypeEnum.DomesticCup => fixture.CupRound?.Cup?.Template?.Name ?? "Unknown Cup",
                    CompetitionTypeEnum.EuropeanCup => fixture.EuropeanCupPhase?.EuropeanCup?.Template?.Name ?? "Unknown European Cup",
                    _ => "Unknown Competition"
                };
            }
            catch
            {
                return "Unknown Competition";
            }
        }

        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatchDetails(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                .Include(m => m.Events)
                    .ThenInclude(e => e.Player)
                .Include(m => m.Events)
                    .ThenInclude(e => e.EventType)
                .Include(m => m.Penalties)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
                return NotFound($"Match with ID {matchId} not found.");

            var dto = new
            {
                id = match.Id,
                date = match.Fixture.Date,
                status = match.Status.ToString(),

                homeTeam = new
                {
                    name = match.Fixture.HomeTeam.Name,
                    logo = match.Fixture.HomeTeam.LogoFileName,
                    goals = match.Events
                        .Where(e => e.TeamId == match.Fixture.HomeTeamId && e.EventType.Code == "GOAL")
                        .Select(e => new
                        {
                            minute = e.Minute,
                            scorer = e.Player != null ? e.Player.FirstName + " " + e.Player.LastName : "Unknown Player"
                        })
                        .ToList()
                },

                awayTeam = new
                {
                    name = match.Fixture.AwayTeam.Name,
                    logo = match.Fixture.AwayTeam.LogoFileName,
                    goals = match.Events
                        .Where(e => e.TeamId == match.Fixture.AwayTeamId && e.EventType.Code == "GOAL")
                        .Select(e => new
                        {
                            minute = e.Minute,
                            scorer = e.Player != null ? e.Player.FirstName + " " + e.Player.LastName : "Unknown Player"
                        })
                        .ToList()
                }
            };

            return Ok(dto);
        }
    }
}