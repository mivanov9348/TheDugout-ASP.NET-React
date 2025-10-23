namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using TheDugout.Data;
    using TheDugout.DTOs.Season;
    using TheDugout.Models.Enums;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Game.Interfaces;
    using TheDugout.Services.Season.Interfaces;
    using TheDugout.Services.User;

    [ApiController]
    [Route("api/[controller]")]
    public class SeasonController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ICompetitionService _competitionService;
        private readonly IGameDayService _gameDayService;
        private readonly IUserContextService _userContext;
        private readonly INewSeasonService _newSeasonService;
        private readonly IEndSeasonService _endSeasonService;
        private readonly ILogger<SeasonController> _logger;

        public SeasonController(DugoutDbContext context, ICompetitionService competitionService, IGameDayService gamedayService, IUserContextService userContext, INewSeasonService newSeasonService, IEndSeasonService endSeasonService, ILogger<SeasonController> logger)
        {
            _context = context;
            _competitionService = competitionService;
            _gameDayService = gamedayService;
            _userContext = userContext;
            _newSeasonService = newSeasonService;
            _endSeasonService = endSeasonService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("{seasonId}/overview")]
        public async Task<IActionResult> GetSeasonOverview(int seasonId)
        {
            _logger.LogInformation("📅 GetSeasonOverview called for seasonId={SeasonId}", seasonId);

            var season = await _context.Seasons.FindAsync(seasonId);
            if (season == null)
                return NotFound();

            var allFinished = await _competitionService.AreAllCompetitionsFinishedAsync(seasonId);

            var results = await _context.CompetitionSeasonResults
                .Include(r => r.Competition).ThenInclude(c => c.League).ThenInclude(l => l.Template)
                .Include(r => r.Competition).ThenInclude(c => c.Cup).ThenInclude(cu => cu.Template)
                .Include(r => r.Competition).ThenInclude(c => c.EuropeanCup).ThenInclude(ec => ec.Template)
                .Include(r => r.ChampionTeam)
                .Include(r => r.RunnerUpTeam)
                .Include(r => r.PromotedTeams).ThenInclude(pt => pt.Team)
                .Include(r => r.RelegatedTeams).ThenInclude(rt => rt.Team)
                .Include(r => r.EuropeanQualifiedTeams).ThenInclude(eq => eq.Team)
                .Include(r => r.Awards).ThenInclude(a => a.Player)
                .Where(r => r.SeasonId == seasonId)
                .ToListAsync();

            var dto = new SeasonOverviewDto
            {
                SeasonId = seasonId,
                AllCompetitionsFinished = allFinished,
                Competitions = new List<CompetitionResultDto>()
            };

            foreach (var r in results)
            {
                var competitionDto = new CompetitionResultDto
                {
                    CompetitionId = r.CompetitionId ?? 0,
                    Name = _competitionService.GetCompetitionNameById(r.CompetitionId ?? 0),
                    Type = r.CompetitionType,
                    ChampionTeam = r.ChampionTeam == null ? null : new TeamSummaryDto
                    {
                        Name = r.ChampionTeam.Name,
                        LogoFileName = r.ChampionTeam.LogoFileName
                    },
                    RunnerUpTeam = r.RunnerUpTeam == null ? null : new TeamSummaryDto
                    {
                        Name = r.RunnerUpTeam.Name,
                        LogoFileName = r.RunnerUpTeam.LogoFileName
                    },
                    PromotedTeams = r.PromotedTeams.Select(t => new TeamSummaryDto
                    {
                        Name = t.Team.Name,
                        LogoFileName = t.Team.LogoFileName
                    }).ToList(),
                    RelegatedTeams = r.RelegatedTeams.Select(t => new TeamSummaryDto
                    {
                        Name = t.Team.Name,
                        LogoFileName = t.Team.LogoFileName
                    }).ToList(),
                    EuropeanQualifiedTeams = r.EuropeanQualifiedTeams.Select(t => new TeamSummaryDto
                    {
                        Name = t.Team.Name,
                        LogoFileName = t.Team.LogoFileName
                    }).ToList(),
                    Awards = r.Awards.Select(a => new AwardDto
                    {
                        PlayerName = $"{a.Player.FirstName} {a.Player.LastName}",
                        AwardType = a.AwardType,
                        Value = a.Value
                    }).ToList()
                };

                // 🏆 Класиране само за лиги
                if (r.CompetitionType == CompetitionTypeEnum.League && r.Competition?.League != null)
                {
                    int leagueId = r.Competition.League.Id;

                    var standings = await _context.LeagueStandings
                        .Include(s => s.Team)
                        .Where(s => s.LeagueId == leagueId && s.SeasonId == seasonId)
                        .OrderByDescending(s => s.Points)
                        .ThenByDescending(s => s.GoalDifference)
                        .ThenByDescending(s => s.GoalsFor)
                        .ToListAsync();

                    competitionDto.LeagueStandings = standings.Select(s => new LeagueStandingDto
                    {
                        TeamName = s.Team.Name,
                        TeamLogo = s.Team.LogoFileName,
                        Points = s.Points,
                        Wins = s.Wins,
                        Draws = s.Draws,
                        Losses = s.Losses,
                        GoalsFor = s.GoalsFor,
                        GoalsAgainst = s.GoalsAgainst,
                        GoalDifference = s.GoalDifference
                    }).ToList();
                }

                // ⚽ Голмайстори за всички
                var topScorers = await _context.PlayerCompetitionStats
                    .Include(p => p.Player)
                    .Where(p => p.SeasonId == seasonId && p.CompetitionId == r.CompetitionId)
                    .OrderByDescending(p => p.Goals)
                    .ThenBy(p => p.MatchesPlayed)
                    .Take(10)
                    .ToListAsync();

                competitionDto.TopScorers = topScorers.Select(ts => new TopScorerDto
                {
                    PlayerName = $"{ts.Player.FirstName} {ts.Player.LastName}",
                    Goals = ts.Goals,
                }).ToList();

                dto.Competitions.Add(competitionDto);
            }


            // 🧠 ЛОКАЛЕН ЛОГ НА ЦЕЛИЯ DTO
            try
            {
                string jsonOutput = JsonConvert.SerializeObject(dto, Formatting.Indented);
                _logger.LogInformation("📤 SeasonOverview DTO sent to frontend:\n{JsonOutput}", jsonOutput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error serializing SeasonOverviewDto for seasonId={SeasonId}", seasonId);
            }

            return Ok(dto);
        }

        [Authorize]
        [HttpPost("season/{seasonId}/end")]
        public async Task<IActionResult> EndSeason(int seasonId, [FromServices] IEndSeasonService endSeasonService)
        {
            Console.WriteLine($"📅 EndSeason called for seasonId={seasonId}");

            try
            {
                var result = await endSeasonService.ProcessSeasonEndAsync(seasonId);

                if (!result)
                {
                    Console.WriteLine($"⚠️ Season {seasonId} cannot end yet.");
                    return BadRequest(new { message = "Not all competitions are finished yet. Season cannot end." });
                }

                Console.WriteLine($"✅ Season {seasonId} ended successfully.");
                return Ok(new { message = "Season ended successfully and next season will be generated." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EndSeason Error: {ex.Message}");
                return StatusCode(500, new { message = "Error while processing season end.", error = ex.Message });
            }
        }

        [HttpPost("{seasonId}/start-new-season")]
        public async Task<IActionResult> StartNewSeason(int seasonId)
        {
            _logger.LogInformation("🚀 Starting a new season based on seasonId={SeasonId}", seasonId);

            try
            {
                var newSeasonId = await _newSeasonService.StartNewSeasonAsync(seasonId);

                _logger.LogInformation("✅ New season created with ID {NewSeasonId}", newSeasonId);
                return Ok(new { newSeasonId });
          
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while starting a new season from seasonId={SeasonId}", seasonId);
                return StatusCode(500, "Failed to start a new season");
            }
        }

        [Authorize]
        [HttpGet("status/{gameSaveId}")]
        public async Task<IActionResult> GetGameStatus(int gameSaveId)
        {
            var save = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(ut => ut.League)
                        .ThenInclude(l => l.Template)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (save == null) return NotFound();

            var season = _context.Seasons.FirstOrDefault(s => s.GameSaveId == gameSaveId && s.IsActive == true);

            if (season == null)
            {
                return Ok(new
                {
                    gameSave = new
                    {
                        save.Id,
                        save.UserTeamId,
                        UserTeam = new
                        {
                            save.UserTeam?.Name,
                            Balance = save.UserTeam?.Balance ?? 0,
                            LeagueName = save.UserTeam?.League?.Template?.Name ?? "Unknown League"
                        },
                        Seasons = Array.Empty<object>()
                    },
                    hasUnplayedMatchesToday = false,
                    hasMatchesToday = false,
                    activeMatch = (object)null
                });
            }

            var today = season.CurrentDate.Date;

            var matches = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Where(f => f.GameSaveId == gameSaveId && f.Date.Date == today)
                .ToListAsync();

            var hasUnplayedMatchesToday = matches.Any(m => m.Status == MatchStageEnum.Scheduled);
            var activeMatch = matches.FirstOrDefault(m => m.Status == MatchStageEnum.Scheduled);
            var hasMatchesToday = matches.Any();

            return Ok(new
            {
                gameSave = new
                {
                    save.Id,
                    save.UserTeamId,
                    UserTeam = new
                    {
                        save.UserTeam.Name,
                        save.UserTeam.Balance,
                        LeagueName = save.UserTeam.League.Template.Name
                    },
                    Seasons = save.Seasons.Select(s => new
                    {
                        s.Id,
                        s.CurrentDate,
                        s.EndDate,
                        s.StartDate
                    }),
                },
                hasUnplayedMatchesToday,
                hasMatchesToday,
                activeMatch = activeMatch != null ? new { activeMatch.Id } : null
            });
        }

        [Authorize]
        [HttpPost("current/next-day")]
        public async Task<IActionResult> NextDay()
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.CurrentSave)
                    .ThenInclude(s => s.Seasons)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user?.CurrentSave == null)
                return BadRequest(new { message = "No current save selected." });

            var saveId = user.CurrentSave.Id;
            var save = user.CurrentSave;

            var season = save.Seasons.FirstOrDefault(s => s.IsActive);
            if (season == null) return BadRequest("No season found.");

            var today = season.CurrentDate.Date;
            var hasUnplayed = await _context.Fixtures
        .AnyAsync(f => f.GameSaveId == saveId && f.Date.Date == today && f.Status == MatchStageEnum.Scheduled);

            if (hasUnplayed)
                return BadRequest(new { message = "Cannot advance day: there are unplayed matches today." });

            var result = await _gameDayService.ProcessNextDayAndGetResultAsync(saveId);
            return Ok(result);
        }

        [HttpGet("current/next-day-stream")]
        public async Task NextDayStream()
        {
            Response.ContentType = "text/event-stream";

            async Task Send(string type, string msg, object? extra = null)
            {
                var payload = JsonConvert.SerializeObject(new { type, message = msg, extra });
                await Response.WriteAsync($"data: {payload}\n\n");
                await Response.Body.FlushAsync();
            }

            try
            {
                var userId = _userContext.GetUserId(User);
                if (userId == null)
                {
                    await Send("error", "Unauthorized");
                    return;
                }

                var user = await _context.Users
                    .Include(u => u.CurrentSave)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (user?.CurrentSave == null)
                {
                    await Send("error", "No current save selected.");
                    return;
                }

                var saveId = user.CurrentSave.Id;

                await Send("progress", "Advancing to next day...");

                await _gameDayService.ProcessNextDayAsync(saveId, msg => Send("progress", msg));

                var result = await _gameDayService.ProcessNextDayAndGetResultAsync(saveId);

                await Send("done", "Finished!", result);
            }
            catch (Exception ex)
            {
                await Send("error", $"Error: {ex.Message}");
            }
        }


    }
}