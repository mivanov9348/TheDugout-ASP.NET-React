namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Dashboard;
    using TheDugout.Services.Transfer.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ITransferQueryService _transferQueryService;

        public DashboardController(DugoutDbContext context, ITransferQueryService transferQueryService)
        {
            _context = context;
            _transferQueryService = transferQueryService;
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
            var transfers = await _transferQueryService.GetTransferHistoryAsync(gameSaveId, true);

            // Текуща дата на сезона
            var season = gameSave.Seasons
                .OrderByDescending(s => s.CurrentDate)
                .FirstOrDefault();

            var currentDate = season?.CurrentDate ?? DateTime.UtcNow;

            // Следващ мач
            var nextMatch = gameSave.Seasons
                .SelectMany(s => s.Fixtures)
                .Where(f => f.Date > currentDate &&
                            (f.HomeTeamId == team.Id || f.AwayTeamId == team.Id))
                .OrderBy(f => f.Date)
                .FirstOrDefault();

            // Последни 5 мача
            var lastFixtures = gameSave.Seasons
                .SelectMany(s => s.Fixtures)
                .Where(f => f.Date < currentDate &&
                            (f.HomeTeamId == team.Id || f.AwayTeamId == team.Id))
                .OrderByDescending(f => f.Date)
                .Take(5)
                .Select(f => new LastFixtureDto
                {
                    Date = f.Date,
                    HomeTeam = f.HomeTeam?.Name ?? "Unknown",
                    AwayTeam = f.AwayTeam?.Name ?? "Unknown",
                    Competition = f.League?.Template.Name ?? "Unknown",
                    HomeGoals = f.HomeTeamGoals,
                    AwayGoals = f.AwayTeamGoals
                })
                .ToList();

            // Standings само за UserTeam
            var standing = await _context.LeagueStandings
                .Include(ls => ls.League)
                    .ThenInclude(l => l.Template)
                .Where(ls => ls.GameSaveId == gameSaveId && ls.TeamId == team.Id)
                .OrderByDescending(ls => ls.Season.CurrentDate)
                .FirstOrDefaultAsync();

            StandingDto? standingDto = null;

            if (standing != null)
            {
                standingDto = new StandingDto
                {
                    League = standing.League.Template.Name,
                    Ranking = standing.Ranking,
                    Matches = standing.Matches,
                    Wins = standing.Wins,
                    Draws = standing.Draws,
                    Losses = standing.Losses,
                    GoalsFor = standing.GoalsFor,
                    GoalsAgainst = standing.GoalsAgainst,
                    GoalDifference = standing.GoalDifference,
                    Points = standing.Points
                };
            }

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
                },
                LastFixtures = lastFixtures,
                Standing = standingDto   // 👈 връщаме само UserTeam standings
            };  

            return Ok(dto);
        }



    }
}
