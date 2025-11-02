namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public FinanceController(DugoutDbContext context)
        {
            _context = context;
        }

        // GET: api/finance/{gameSaveId}
        [HttpGet("{gameSaveId}")]
        public async Task<IActionResult> GetFinance(int gameSaveId)
        {
            var save = await _context.GameSaves
                .Include(gs => gs.UserTeam)
                .ThenInclude(t => t.TransactionsFrom)
                .Include(gs => gs.UserTeam)
                .ThenInclude(t => t.TransactionsTo)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (save == null || save.UserTeam == null)
                return NotFound("GameSave or UserTeam not found.");

            var team = save.UserTeam;

            var transactions = team.TransactionsFrom
                .Concat(team.TransactionsTo)
                .OrderByDescending(t => t.Date)
                .Select(t => new
                {
                    t.Id,
                    t.Date,
                    t.Description,
                    Amount = t.FromTeamId == team.Id ? -t.Amount : t.Amount,
                    t.Type,
                    t.Status
                });

            return Ok(new
            {
                Balance = team.Balance,
                Transactions = transactions
            });
        }
    }
}
