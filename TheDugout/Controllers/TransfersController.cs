namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Transfer;
    using TheDugout.Services.Transfer;

    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly DugoutDbContext _context;

        public TransfersController(DugoutDbContext context, ITransferService transferService)
        {
            _context = context;
            _transferService = transferService;
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
            var result = await _transferService.GetPlayersAsync(
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

            var (success, errorMessage) = await _transferService.BuyPlayerAsync(
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
            var result = await _transferService.SendOfferAsync(request);
            if (!result.Success)
                return BadRequest(new { success = false, error = result.ErrorMessage });

            return Ok(new { success = true, message = "Offer sent successfully." });
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetTransferHistory(int gameSaveId, bool onlyMine = false)
        {
            var transfers = await _transferService.GetTransferHistoryAsync(gameSaveId, onlyMine);
            return Ok(transfers);
        }
    }
}
