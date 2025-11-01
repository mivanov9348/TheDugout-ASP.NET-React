namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheDugout.Services.Player.Interfaces;
    [ApiController]
    [Route("api/player")] 
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

        [HttpGet("academy/{gameSaveId}")]
        public async Task<IActionResult> GetAcademyPlayers(int gameSaveId)
        {
            var players = await _playerService.GetAllPlayersAsync(gameSaveId);

            var academyPlayers = players
                .Where(p => p.YouthProfile != null && !p.YouthProfile.IsPromoted)
                .Select(p => new
                {
                    p.Id,
                    Name = p.FirstName + " " + p.LastName,
                    p.Age,
                    Position = p.Position != null ? p.Position.Name : "N/A",
                    Country = p.Country != null ? p.Country.Name : "Unknown",
                    Nationality = p.Country != null ? p.Country.FlagEmoji : "🏳️",
                    p.CurrentAbility,
                    p.PotentialAbility,
                    Photo = $"/images/players/{p.AvatarFileName}",
                    Appearances = p.SeasonStats.Sum(s => s.Appearances),
                    Goals = p.SeasonStats.Sum(s => s.Goals),
                    Assists = p.SeasonStats.Sum(s => s.Assists)
                })
                .ToList();

            return Ok(academyPlayers);
        }
    }
}
