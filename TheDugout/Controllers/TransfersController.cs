namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Transfer;
    using TheDugout.Services.Transfer.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ITransferQueryService _transferQueryService;
        private readonly IFreeAgentTransferService _freeAgentTransferService;
        private readonly IClubToClubTransferService _clubToClubTransferService;


        public TransfersController(DugoutDbContext context, ITransferQueryService transferQueryService, IFreeAgentTransferService freeAgentTransferService, IClubToClubTransferService clubToClubTransferService)
        {
            _context = context;
            _transferQueryService = transferQueryService;
            _freeAgentTransferService = freeAgentTransferService;
            _clubToClubTransferService = clubToClubTransferService;
        }


        [HttpGet("userteam/{gameSaveId}")]
        public async Task<IActionResult> GetUserTeam(int gameSaveId)
        {
            var gameSave = await _context.GameSaves
                .Include(g => g.UserTeam)
                .FirstOrDefaultAsync(g => g.Id == gameSaveId);

            if (gameSave == null)
                return NotFound(new { error = "Game save not found." });

            if (gameSave.UserTeam == null)
                return Ok(new { userTeamId = (int?)null, userTeamName = (string?)null });

            return Ok(new
            {
                userTeamId = gameSave.UserTeam.Id,
                userTeamName = gameSave.UserTeam.Name
            });
        }
        [HttpGet("players")]

        public async Task<IActionResult> GetPlayers(
    int gameSaveId,
    string? search = null,
    string? team = null,
    string? country = null,
    string? position = null,
    bool freeAgent = false,
    string sortBy = "name",
    string sortOrder = "asc",
    int page = 1,
    int pageSize = 15,
    int? minAge = null,
    int? maxAge = null,
    decimal? minPrice = null,
    decimal? maxPrice = null)
        {
            var result = await _transferQueryService.GetPlayersAsync(
                gameSaveId, search, team, country, position, freeAgent,
                sortBy, sortOrder, page, pageSize,
                minAge, maxAge, minPrice, maxPrice);

            return Ok(result);
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyPlayer([FromBody] BuyPlayerRequest request)
        {
            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(g => g.Id == request.GameSaveId);

            if (gameSave == null)
                return NotFound(new { success = false, error = "Game save not found." });

            if (gameSave.UserTeamId == null)
                return BadRequest(new { success = false, error = "This save has no assigned team." });

            var (success, errorMessage) = await _freeAgentTransferService.SignFreePlayer(
                request.GameSaveId,
                gameSave.UserTeamId.Value,
                request.PlayerId
            );

            if (!success)
                return BadRequest(new { success = false, error = errorMessage });

            return Ok(new { success = true });
        }

        [HttpPost("release")]
        public async Task<IActionResult> ReleasePlayer([FromBody] ReleasePlayerRequest request)
        {
            var gameSave = await _context.GameSaves.FirstOrDefaultAsync(g => g.Id == request.GameSaveId);
            if (gameSave == null)
                return NotFound(new { success = false, error = "Game save not found." });

            if (gameSave.UserTeamId == null)
                return BadRequest(new { success = false, error = "This save has no assigned team." });

            var (success, errorMessage) = await _freeAgentTransferService.ReleasePlayerAsync(
                request.GameSaveId,
                gameSave.UserTeamId.Value,
                request.PlayerId
            );

            if (!success)
                return BadRequest(new { success = false, error = errorMessage });

            return Ok(new { success = true });
        }

        [HttpPost("offer")]
        public async Task<IActionResult> SendTransferOffer([FromBody] TransferOfferRequest request)
        {
            var result = await _clubToClubTransferService.SendOfferAsync(request);
            if (!result.Success)
                return BadRequest(new { success = false, error = result.ErrorMessage });

            return Ok(new { success = true, message = "Offer sent successfully." });
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetTransferHistory(int gameSaveId, bool onlyMine = false)
        {
            var transfers = await _transferQueryService.GetTransferHistoryAsync(gameSaveId, onlyMine);
            return Ok(transfers);
        }
    }
}
