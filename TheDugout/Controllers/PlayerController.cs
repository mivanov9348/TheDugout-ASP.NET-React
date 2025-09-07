using Microsoft.AspNetCore.Mvc;
using TheDugout.Services.Interfaces;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            if (player == null) return NotFound();

            var attributes = await _playerService.GetPlayerAttributesAsync(id);
            var stats = await _playerService.GetPlayerSeasonStatsAsync(id);

            var result = new
            {
                player.Id,
                player.FullName,
                player.Age,
                player.Position,
                player.TeamName,
                player.Country,
                player.HeightCm,
                player.WeightKg,
                player.Price,
                Attributes = attributes,
                SeasonStats = stats
            };

            return Ok(result);
        }
    }
}
