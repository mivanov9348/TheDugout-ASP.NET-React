using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public GameController(
            ITemplateService templateService,
            IGameSaveService gameSaveService,
            IUserContextService userContext)
        {
            _templateService = templateService;
            _gameSaveService = gameSaveService;
            _userContext = userContext;
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
    }
}
