namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Enums;

    [ApiController]
    [Route("api/[controller]")]
    public class ClubsController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public ClubsController(DugoutDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clubs = await _context.Teams
                .Include(t => t.League)
                .ThenInclude(l => l.Template)
                .Include(t => t.Country)
                .Include(t => t.Stadium)
                .Include(t => t.TrainingFacility)
                .Include(t => t.YouthAcademy)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.LogoFileName,
                    t.Abbreviation,
                    League = t.League != null ? new { t.League.Id, t.League.Template.Name } : null,
                    Country = t.Country != null ? new { t.Country.Id, t.Country.Name } : null,
                    TrainingFacility = t.TrainingFacility != null ? new { t.TrainingFacility.Level } : null,
                    YouthAcademy = t.YouthAcademy != null ? new { t.YouthAcademy.Level } : null,
                    t.Balance,
                    t.Popularity,
                    PlayerCount = t.Players.Count
                })
                .ToListAsync();

            return Ok(clubs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var club = await _context.Teams
                .Include(t => t.League)
                .ThenInclude(l => l.Template)
                .Include(t => t.Country)
                .Include(t => t.Stadium)
                .Include(t => t.TrainingFacility)
                .Include(t => t.YouthAcademy)
                .Include(t => t.Players)
                .Include(t => t.TeamTactic)
                .ThenInclude(tt => tt.Tactic)
                .Include(t => t.GameSave)
                .ThenInclude(gs => gs.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (club == null) return NotFound();

            var result = new
            {
                club.Id,
                club.Name,
                club.LogoFileName,
                club.Abbreviation,
                League = club.League != null ? new { club.League.Id, Name = club.League.Template.Name } : null,
                Country = club.Country != null ? new { club.Country.Id, club.Country.Name } : null,
                TrainingFacility = club.TrainingFacility != null ? new { club.TrainingFacility.Level } : null,
                YouthAcademy = club.YouthAcademy != null ? new { club.YouthAcademy.Level } : null,
                club.Balance,
                club.Popularity,
                Players = club.Players.Select(p => new
                {
                    p.Id,
                    Name = $"{p.FirstName} {p.LastName}",
                    p.Position,
                    p.Age,
                }),
                Tactic = club.TeamTactic != null ? new { club.TeamTactic.Tactic.Name } : null,
                Manager = club.GameSave?.User?.Username
               
            };

            return Ok(result);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetClubStats(int id)
        {
            var club = await _context.Teams
                .Include(t => t.HomeFixtures)
                .Include(t => t.AwayFixtures)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (club == null) return NotFound();

            // Calculate basic stats from fixtures - only completed matches
            var allFixtures = club.HomeFixtures.Concat(club.AwayFixtures)
                .Where(f => f.Status == FixtureStatusEnum.Played);

            var matchesPlayed = allFixtures.Count();
            var wins = allFixtures.Count(f =>
                (f.HomeTeamId == id && f.HomeTeamGoals > f.AwayTeamGoals) ||
                (f.AwayTeamId == id && f.AwayTeamGoals > f.HomeTeamGoals));
            var draws = allFixtures.Count(f => f.HomeTeamGoals == f.AwayTeamGoals);
            var losses = matchesPlayed - wins - draws;

            var stats = new
            {
                MatchesPlayed = matchesPlayed,
                Wins = wins,
                Draws = draws,
                Losses = losses,
                WinPercentage = matchesPlayed > 0 ? (decimal)wins * 100 / matchesPlayed : 0,
                GoalsFor = allFixtures.Sum(f => f.HomeTeamId == id ? f.HomeTeamGoals ?? 0 : f.AwayTeamGoals ?? 0),
                GoalsAgainst = allFixtures.Sum(f => f.HomeTeamId == id ? f.AwayTeamGoals ?? 0 : f.HomeTeamGoals ?? 0),
                GoalDifference = allFixtures.Sum(f =>
                    (f.HomeTeamId == id ?
                     (f.HomeTeamGoals ?? 0) - (f.AwayTeamGoals ?? 0) :
                     (f.AwayTeamGoals ?? 0) - (f.HomeTeamGoals ?? 0)))
            };

            return Ok(stats);
        }

        [HttpGet("{id}/fixtures")]
        public async Task<IActionResult> GetClubFixtures(int id, [FromQuery] int limit = 5)
        {
            var fixtures = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.League)
                .ThenInclude(l => l.Template)
                .Where(f => (f.HomeTeamId == id || f.AwayTeamId == id) && f.Status != FixtureStatusEnum.Played)
                .OrderBy(f => f.Date)
                .Take(limit)
                .Select(f => new
                {
                    f.Id,
                    MatchDate = f.Date,
                    HomeTeam = new { f.HomeTeam.Id, f.HomeTeam.Name, f.HomeTeam.LogoFileName },
                    AwayTeam = new { f.AwayTeam.Id, f.AwayTeam.Name, f.AwayTeam.LogoFileName },
                    League = f.League != null ? new { f.League.Id, Name = f.League.Template.Name } : null,
                    f.HomeTeamGoals,
                    f.AwayTeamGoals,
                    Status = f.Status.ToString(),
                    IsCompleted = f.Status == FixtureStatusEnum.Played
                })
                .ToListAsync();

            return Ok(fixtures);
        }

        [HttpGet("{id}/recent-matches")]
        public async Task<IActionResult> GetRecentMatches(int id, [FromQuery] int limit = 5)
        {
            var matches = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.League)
                .ThenInclude(l => l.Template)
                .Where(f => (f.HomeTeamId == id || f.AwayTeamId == id) && f.Status == FixtureStatusEnum.Played)
                .OrderByDescending(f => f.Date)
                .Take(limit)
                .Select(f => new
                {
                    f.Id,
                    MatchDate = f.Date,
                    HomeTeam = new { f.HomeTeam.Id, f.HomeTeam.Name, f.HomeTeam.LogoFileName },
                    AwayTeam = new { f.AwayTeam.Id, f.AwayTeam.Name, f.AwayTeam.LogoFileName },
                    League = f.League != null ? new { f.League.Id, Name = f.League.Template.Name } : null,
                    f.HomeTeamGoals,
                    f.AwayTeamGoals,
                    Result = f.HomeTeamId == id ?
                        (f.HomeTeamGoals > f.AwayTeamGoals ? "W" : f.HomeTeamGoals == f.AwayTeamGoals ? "D" : "L") :
                        (f.AwayTeamGoals > f.HomeTeamGoals ? "W" : f.AwayTeamGoals == f.HomeTeamGoals ? "D" : "L")
                })
                .ToListAsync();

            return Ok(matches);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchClubs([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return BadRequest("Search query must be at least 2 characters long");

            var clubs = await _context.Teams
                .Include(t => t.League)
                .ThenInclude(l => l.Template)
                .Include(t => t.Country)
                .Where(t => t.Name.Contains(query) ||
                           t.Abbreviation.Contains(query) ||
                           (t.League != null && t.League.Template.Name.Contains(query)))
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.LogoFileName,
                    t.Abbreviation,
                    League = t.League != null ? t.League.Template.Name : "No League",
                    Country = t.Country != null ? t.Country.Name : "Unknown",
                    t.Popularity
                })
                .OrderByDescending(t => t.Popularity)
                .Take(10)
                .ToListAsync();

            return Ok(clubs);
        }

        [HttpGet("{id}/squad")]
        public async Task<IActionResult> GetClubSquad(int id)
        {
            var players = await _context.Players
                .Where(p => p.TeamId == id)
                .OrderByDescending(p => p.Position.Id)
                .ThenBy(p => p.Position)
                .Select(p => new
                {
                    p.Id,
                    Name = $"{p.FirstName} {p.LastName}",
                    p.Position,
                    p.Age,                  
                    p.Country,
                    p.Price         
                })
                .ToListAsync();

            return Ok(players);
        }

        [HttpGet("{id}/finances")]
        public async Task<IActionResult> GetClubFinances(int id)
        {
            var club = await _context.Teams
                .Include(t => t.TransactionsFrom)
                .Include(t => t.TransactionsTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (club == null) return NotFound();

            var recentTransactions = club.TransactionsFrom
                .Concat(club.TransactionsTo)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .Select(t => new
                {
                    t.Id,
                    t.Amount,
                    t.Type,
                    t.Description,
                    t.Date,
                    IsIncome = t.ToTeamId == id
                });

            var finances = new
            {
                club.Balance,
                SquadValue = club.Players.Sum(p => p.Price),
                RecentTransactions = recentTransactions
            };

            return Ok(finances);
        }
    }
}