namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Services.Player.Interfaces;

    [ApiController]
    [Route("api/player")] 
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerInfoService _playerService;
        private readonly IYouthPlayerService _youthPlayerService;
        private readonly IShortlistPlayerService _shortlistService;
        private readonly DugoutDbContext _context;

        public PlayerController(IPlayerInfoService playerService, IYouthPlayerService youthPlayerService, IShortlistPlayerService shortlistService, DugoutDbContext context)
        {
            _playerService = playerService;
            _youthPlayerService = youthPlayerService;
            _shortlistService = shortlistService;
            _context = context;
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

            return Ok(players);
        }

        [HttpPost("academy/promote/{playerId}")]
        public async Task<IActionResult> PromotePlayer(int playerId)
        {
            var player = await _youthPlayerService.GetYouthPlayerByIdAsync(playerId);
            if (player == null)
                return NotFound(new { message = "Player not found in academy." });

            var age = player.Player.Age;
            if (age < 18)
                return BadRequest(new { message = $"Player {player.Player.FirstName} {player.Player.LastName} is too young ({age}) to be promoted." });

            // Mark as promoted
            player.IsPromoted = true;

            // Move to senior team
            player.Player.TeamId = player.YouthAcademy.TeamId; // assuming academy has a parent team
            player.YouthAcademyId = 0;

            await _youthPlayerService.UpdateYouthPlayerAsync(player);

            return Ok(new { message = $"Player {player.Player.FirstName} {player.Player.LastName} has been promoted to the senior team." });
        }

        [HttpDelete("academy/release/{playerId}")]
        public async Task<IActionResult> ReleasePlayer(int playerId)
        {
            var player = await _youthPlayerService.GetYouthPlayerByIdAsync(playerId);
            if (player == null)
                return NotFound(new { message = "Player not found in academy." });

            await _youthPlayerService.DeleteYouthPlayerAsync(playerId);

            return Ok(new { message = $"Player {player.Player.FirstName} {player.Player.LastName} has been released from the academy." });
        }

        [HttpPost("{playerId}/shortlist")]
        public async Task<IActionResult> AddToShortlist(int playerId, [FromQuery] int gameSaveId)
        {
            var gameSave = await _context.GameSaves
                .Include(g => g.User)
                .Include(g => g.UserTeam)
                .FirstOrDefaultAsync(g => g.Id == gameSaveId);

            if (gameSave == null)
                return NotFound("Game save not found.");

            var userId = gameSave.UserId;
            var teamId = gameSave.UserTeamId;

            await _shortlistService.AddToShortlistAsync(gameSaveId, playerId, userId, teamId);
            return Ok(new { message = "Player added to shortlist." });
        }

        [HttpDelete("{playerId}/shortlist")]
        public async Task<IActionResult> RemoveFromShortlist(int playerId, [FromQuery] int gameSaveId)
        {
            var gameSave = await _context.GameSaves
                .Include(g => g.User)
                .Include(g => g.UserTeam)
                .FirstOrDefaultAsync(g => g.Id == gameSaveId);

            if (gameSave == null)
                return NotFound("Game save not found.");

            var userId = gameSave.UserId;
            var teamId = gameSave.UserTeamId;

            await _shortlistService.RemoveFromShortlistAsync(gameSaveId, playerId, userId, teamId);
            return Ok(new { message = "Player removed from shortlist." });
        }

        [HttpGet("GetShortlist")]
        public async Task<IActionResult> GetShortlist([FromQuery] int gameSaveId)
        {
            var players = await _shortlistService.GetShortlistPlayersAsync(gameSaveId);
            return Ok(players);
        }

    }
}
