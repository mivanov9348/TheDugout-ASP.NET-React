using Microsoft.AspNetCore.Mvc;
using TheDugout.Services.Player.Interfaces;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/player")] // <-- фиксираме route-а с малки букви
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerInfoService _playerService;

        public PlayerController(IPlayerInfoService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null)
                return NotFound();

            return Ok(player);
        }
    }
}
