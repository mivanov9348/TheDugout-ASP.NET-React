using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Data.DtoNewGame;
using TheDugout.Services.Game;
using TheDugout.Services.Template;
using TheDugout.Services.User;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GameController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly IGameSaveService _gameSaveService;
        private readonly IUserContextService _userContext;
        private readonly DugoutDbContext _context;

        public GameController(
            ITemplateService templateService,
            IGameSaveService gameSaveService,
            IUserContextService userContext,
            DugoutDbContext _context)
        {
            _templateService = templateService;
            _gameSaveService = gameSaveService;
            _userContext = userContext;
            this._context = _context;
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

            var success = await _gameSaveService.DeleteGameSaveAsync(userId.Value, id);
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
                var save = await _gameSaveService.StartNewGameAsync(userId.Value);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGameSave(int id)
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var gameSave = await _gameSaveService.GetGameSaveAsync(userId.Value, id);
            return gameSave == null ? NotFound(new { message = "Save not found" })
                                    : Ok(gameSave.ToDto());
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSave()
        {
            var userId = _userContext.GetUserId(User);
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.CurrentSave)
                .ThenInclude(gs => gs.Seasons)
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

            var user = await _context.Users.FirstAsync(u => u.Id == userId.Value);
            user.CurrentSaveId = save.Id;

            await _context.SaveChangesAsync();

            return Ok(save.ToDto());
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
