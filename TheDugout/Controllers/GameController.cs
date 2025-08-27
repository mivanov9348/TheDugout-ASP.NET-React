using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheDugout.Data;
using TheDugout.Data.DtoNewGame;
using TheDugout.Models;


namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/games")]

    public class GameController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameController> _logger;


        public GameController(DugoutDbContext context, ILogger<GameController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET api/games/teamtemplates
        [HttpGet("teamtemplates")]
        public async Task<IActionResult> GetTeamTemplates()
        {
            var list = await _context.TeamTemplates
                       .AsNoTracking()
                       .Include(t => t.League)
                       .Select(t => new TeamTemplateDto
                       {
                           Id = t.Id,
                           Name = t.Name,
                           Abbreviation = t.Abbreviation,
                           CountryId = t.CountryId,
                           LeagueId = t.LeagueId,
                           LeagueName = t.League.Name,  
                           Tier = t.League.Tier
                       })
                       .ToListAsync();

            return Ok(list);
        }

        [HttpPost("new")]
        public async Task<IActionResult> StartNewGame([FromBody] NewGameRequest req)
        {
            // 1) Get userId from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User id not found in token" });
            }


            // 2) Validate template
            var chosenTemplate = await _context.TeamTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == req.TeamTemplateId);


            if (chosenTemplate == null)
                return BadRequest(new { message = "team template not found" });


            // 3) Create GameSave (not yet saved)
            var gameSave = new GameSave
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
            };


            // 4) Copy league templates -> leagues
            var leagueTemplates = await _context.LeagueTemplates.AsNoTracking().ToListAsync();
            foreach (var lt in leagueTemplates)
            {
                var league = new League
                {
                    TemplateId = lt.Id,
                    GameSave = gameSave,
                    CountryId = lt.CountryId,
                    Tier = lt.Tier,
                    TeamsCount = lt.TeamsCount,
                    RelegationSpots = lt.RelegationSpots,
                    PromotionSpots = lt.PromotionSpots
                };
                gameSave.Leagues.Add(league);
            }
            // 5) Copy team templates -> teams
            var teamTemplates = await _context.TeamTemplates.AsNoTracking().ToListAsync();
            var createdTeams = new List<Team>();
            foreach (var tt in teamTemplates)
            {
                var team = new Team
                {
                    TemplateId = tt.Id,
                    GameSave = gameSave,
                    Name = tt.Name,
                    Abbreviation = tt.Abbreviation,
                    CountryId = tt.CountryId,
                    Points = 0,
                    Matches = 0,
                    Wins = 0,
                    Draws = 0,
                    Losses = 0,
                    GoalsFor = 0,
                    GoalsAgainst = 0,
                    GoalDifference = 0
                };
                createdTeams.Add(team);
                gameSave.Teams.Add(team);
            }


            // 6) Разпределяме отборите в лиги (по CountryId & свободно място)
            foreach (var team in createdTeams)
            {
                var leagueCandidate = gameSave.Leagues
                .FirstOrDefault(l => l.CountryId == team.CountryId && l.Teams.Count < l.TeamsCount);


                if (leagueCandidate == null)
                {
                    // fallback: първата лига с място
                    leagueCandidate = gameSave.Leagues.FirstOrDefault(l => l.Teams.Count < l.TeamsCount);
                }


                if (leagueCandidate != null)
                {
                    leagueCandidate.Teams.Add(team);
                    // ако имаш Team.LeagueId/League - може да се сетне тук
                    team.League = leagueCandidate;
                }
            }
            // 7) Save everything в една транзакция
            _context.GameSaves.Add(gameSave);
            await _context.SaveChangesAsync();


            // 8) Намери в новосъздадените teams този, който е съответствие на избора и set-ни UserTeamId
            var userTeam = await _context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.GameSaveId == gameSave.Id && t.TemplateId == req.TeamTemplateId);


            if (userTeam == null)
            {
                // странен случай — върни информация но логни
                _logger.LogWarning("Selected template created but team not found in created teams. GameSaveId={GameSaveId}", gameSave.Id);
                return Ok(new NewGameResponse { GameSaveId = gameSave.Id, UserTeamId = 0, UserTeamName = "" });
            }


            // 9) Update GameSave.UserTeamId
            var gs = await _context.GameSaves.FindAsync(gameSave.Id);
            gs.UserTeamId = userTeam.Id;
            await _context.SaveChangesAsync();


            // 10) Връщаме минимални данни към фронтенда
            var resp = new NewGameResponse
            {
                GameSaveId = gameSave.Id,
                UserTeamId = userTeam.Id,
                UserTeamName = userTeam.Name
            };


            return Ok(resp);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out successfully" });
        }
    }
}