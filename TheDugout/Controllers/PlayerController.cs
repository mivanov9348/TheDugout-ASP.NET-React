namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TheDugout.Services.Player.Interfaces;

    [ApiController]
    [Route("api/player")] 
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerInfoService _playerService;
        private readonly IYouthPlayerService _youthPlayerService;

        public PlayerController(IPlayerInfoService playerService, IYouthPlayerService youthPlayerService)
        {
            _playerService = playerService;
            _youthPlayerService = youthPlayerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null)
                return NotFound();

            return Ok(player);
        }

        [HttpGet("academy/team/{teamId}")]
        public async Task<IActionResult> GetAcademyPlayersByTeam(int teamId)
        {
            var players = await _youthPlayerService.GetYouthPlayersByTeamAsync(teamId);

            if (players == null || !players.Any())
                return Ok(new List<object>());

            var result = players.Select(p => new
            {
                Id = p.Id,
                Name = p.FirstName + " " + p.LastName,
                Age = p.Age,
                Position = p.Position?.Name ?? "N/A",
                Nationality = p.Country,
                Country = p.Country?.Name ?? "Unknown",
                CurrentAbility = p.CurrentAbility,
                PotentialAbility = p.PotentialAbility,
                Photo = $"/images/players/{p.AvatarFileName}",
                Goals = p.SeasonStats?.Sum(s => s.Goals) ?? 0,
            });

            return Ok(result);
        }
     
    }
}
