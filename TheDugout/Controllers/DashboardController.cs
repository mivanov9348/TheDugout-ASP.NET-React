using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.Dashboard;
using TheDugout.Services.Transfer;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ITransferService _transferService;

        public DashboardController(DugoutDbContext context, ITransferService _transferService)
        {
            _context = context;
            this._transferService = _transferService;
        }

        [HttpGet("{gameSaveId}")]
        public async Task<ActionResult<DashboardDto>> GetDashboard(int gameSaveId)
        {
            var gameSave = await _context.GameSaves
    .Include(gs => gs.UserTeam)
        .ThenInclude(t => t.TransactionsFrom)
    .Include(gs => gs.UserTeam)
        .ThenInclude(t => t.TransactionsTo)
    .Include(gs => gs.Seasons)
        .ThenInclude(s => s.Fixtures)
            .ThenInclude(f => f.HomeTeam)
    .Include(gs => gs.Seasons)
        .ThenInclude(s => s.Fixtures)
            .ThenInclude(f => f.AwayTeam)
    .Include(gs => gs.Seasons)
        .ThenInclude(s => s.Fixtures)
            .ThenInclude(f => f.League)
                .ThenInclude(l => l.Template)
    .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);


            if (gameSave == null || gameSave.UserTeam == null)
                return NotFound("GameSave или UserTeam не е намерен.");

            var team = gameSave.UserTeam;

            // Финанси
            var transactions = team.TransactionsFrom
                .Concat(team.TransactionsTo)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .Select(t => new TransactionDto
                {
                    Date = t.Date,
                    Amount = t.Amount,
                    Description = t.Description,
                    Type = t.Type.ToString(),
                    Status = t.Status.ToString()
                })
                .ToList();

            // Трансфери
            var transfers = await _transferService.GetTransferHistoryAsync(gameSaveId, true);

            // Следващ мач
            var nextMatch = gameSave.Seasons
                .SelectMany(s => s.Fixtures)
                                .OrderBy(f => f.Date)
                .FirstOrDefault();

            var dto = new DashboardDto
            {
                Finance = new FinanceDto
                {
                    CurrentBalance = team.Balance,
                    RecentTransactions = transactions
                },
                Transfers = transfers.Select(t => new TransferHistoryDto
                {
                    Id = (int)t.GetType().GetProperty("Id")!.GetValue(t)!,
                    Player = (string)t.GetType().GetProperty("Player")!.GetValue(t)!,
                    FromTeam = (string)t.GetType().GetProperty("FromTeam")!.GetValue(t)!,
                    ToTeam = (string)t.GetType().GetProperty("ToTeam")!.GetValue(t)!,
                    Fee = (decimal)t.GetType().GetProperty("Fee")!.GetValue(t)!,
                    GameDate = (DateTime)t.GetType().GetProperty("GameDate")!.GetValue(t)!,
                    IsFreeAgent = (bool)t.GetType().GetProperty("IsFreeAgent")!.GetValue(t)!,
                    Season = (string)t.GetType().GetProperty("Season")!.GetValue(t)!
                }).ToList(),
                NextMatch = nextMatch == null ? null : new NextMatchDto
                {
                    Date = nextMatch.Date,
                    HomeTeam = nextMatch.HomeTeam?.Name ?? "Unknown",
                    AwayTeam = nextMatch.AwayTeam?.Name ?? "Unknown",
                    Competition = nextMatch.League?.Template.Name ?? "Unknown"
                }
            };

            return Ok(dto);
        }
    }
}
