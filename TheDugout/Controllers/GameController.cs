namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Messages;
    using TheDugout.Services.Game.Interfaces;
    using TheDugout.Services.Message.Interfaces;
    using TheDugout.Services.Template;
    using TheDugout.Services.User;

    [ApiController]
    [Route("api/games")]
    public class GameController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly IGameSaveService _gameSaveService;
        private readonly IUserContextService _userContext;
        private readonly IMessageOrchestrator _messageOrchestrator;
        private readonly DugoutDbContext _context;

        public GameController(
            ITemplateService templateService,
            IGameSaveService gameSaveService,
            IUserContextService userContext,
            DugoutDbContext context,
            IMessageService messageService,
            IMessageOrchestrator messageOrchestrator,
            IGameDayService gameDayService)
        {
            _templateService = templateService;
            _gameSaveService = gameSaveService;
            _userContext = userContext;
            _context = context;
            _messageOrchestrator = messageOrchestrator;
        }
       
        [HttpGet("teamtemplates")]
        public async Task<IActionResult> GetTeamTemplates()
        {
            var list = await _templateService.GetTeamTemplatesAsync();
            return Ok(list);
        }

        [Authorize]
        [HttpGet("saves")]
        public async Task<IActionResult> GetUserGameSaves()
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var saves = await _gameSaveService.GetUserSavesAsync(userId.Value);
            return Ok(saves);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGameSave(int id)
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var success = await _gameSaveService.DeleteGameSaveAsync(id);
            return success ? Ok(new { message = "Game save deleted successfully" })
                           : NotFound(new { message = "Save not found" });
        }

        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> StartNewGame()
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            try
            {
                var save = await _gameSaveService.StartNewGameAsync(userId.Value, default);
                var user = await _context.Users.FirstAsync(u => u.Id == userId.Value);
                user.CurrentSaveId = null;
                await _context.SaveChangesAsync();
                return Ok(save.ToDto());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to create new game" });
            }
        }
       
        [Authorize]
        [HttpPost("{saveId}/select-team/{teamId}")]
        public async Task<IActionResult> SelectTeam(int saveId, int teamId)
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var save = await _context.GameSaves
                .AsSplitQuery()
                .Include(gs => gs.Teams)
                .Include(gs => gs.UserTeam)
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId.Value);

            if (save == null)
                return NotFound(new { message = "Save not found" });

            if (save.UserTeamId != null)
                return BadRequest(new { message = "Team already selected for this save" });

            var team = save.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
                return NotFound(new { message = "Team not found in this save" });

            save.UserTeamId = team.Id;
            await _context.SaveChangesAsync();

            // User и Team инфо
            var user = await _context.Users.FirstAsync(u => u.Id == userId.Value);
            var selectedTeam = await _context.Teams
                              .Include(t => t.Country)
                              .FirstAsync(t => t.Id == teamId);

            await _messageOrchestrator.SendMessageAsync(MessageCategory.Welcome, save.Id, (user, selectedTeam));

            var fullSave = await _context.GameSaves
                .AsSplitQuery()
                .Include(gs => gs.UserTeam).ThenInclude(t => t.Country)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Country)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Template)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Country)
                .Include(gs => gs.Seasons)
                .FirstAsync(gs => gs.Id == save.Id);

            return Ok(fullSave.ToDto());
        }

        [Authorize]
        [HttpGet("{saveId}")]
        public async Task<IActionResult> GetGameSave(int saveId)
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var gameSave = await _context.GameSaves
                .AsSplitQuery()
                .Include(gs => gs.UserTeam).ThenInclude(t => t.Country)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Country)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Template)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Country)
                .Include(gs => gs.Seasons)
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId.Value);

            return gameSave == null
                ? NotFound(new { message = "Save not found" })
                : Ok(gameSave.ToDto());
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSave()
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .AsSplitQuery()
                .Include(u => u.CurrentSave).ThenInclude(gs => gs.UserTeam).ThenInclude(t => t.Country)
                .Include(u => u.CurrentSave).ThenInclude(gs => gs.Leagues).ThenInclude(l => l.Country)
                .Include(u => u.CurrentSave).ThenInclude(gs => gs.Leagues).ThenInclude(l => l.Template)
                .Include(u => u.CurrentSave).ThenInclude(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Country)
                .Include(u => u.CurrentSave).ThenInclude(gs => gs.Seasons).ThenInclude(s => s.Events)
                .Include(u => u.CurrentSave).ThenInclude(gs => gs.Seasons).ThenInclude(s => s.Fixtures)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user?.CurrentSave == null)
                return Ok(null);

            return Ok(user.CurrentSave.ToDto());
        }

        [Authorize]
        [HttpPost("current/{id}")]
        public async Task<IActionResult> SetCurrentSave(int id)
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var save = await _context.GameSaves.FirstOrDefaultAsync(gs => gs.Id == id && gs.UserId == userId.Value);
            if (save == null)
                return NotFound(new { message = "Save not found" });

            if (save.UserTeamId == null)
                return BadRequest(new { message = "Select a team before entering the game." });

            var user = await _context.Users.FirstAsync(u => u.Id == userId.Value);
            user.CurrentSaveId = save.Id;
            await _context.SaveChangesAsync();

            var full = await _context.GameSaves
                .AsSplitQuery()
                .Include(gs => gs.UserTeam)
                .Include(gs => gs.Leagues).ThenInclude(l => l.Teams)
                .Include(gs => gs.Seasons)
                .FirstAsync(gs => gs.Id == save.Id);

            return Ok(full.ToDto());
        }

        [Authorize]
        [HttpPost("exit")]
        public async Task<IActionResult> ExitGame()
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null) return NotFound();

            user.CurrentSaveId = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exited game successfully" });
        }
    }
}
