namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;

    [ApiController]
    [Route("api/[controller]")]
    public class LeagueController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public LeagueController(DugoutDbContext context)
        {
            _context = context;
        }

        [HttpGet("{gameSaveId}")]
        public async Task<IActionResult> GetLeaguesByGameSave(int gameSaveId)
        {
            var season = await _context.Seasons
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (season == null)
                return NotFound("❌ No active season found.");

            var leagues = await _context.Leagues
                .Include(l => l.Template)
                .Include(l => l.Country)
                .Where(l => l.GameSaveId == gameSaveId && l.SeasonId == season.Id)
                .Select(l => new
                {
                    id = l.Id,
                    name = l.Template.Name,
                    country = l.Country.Name,
                    tier = l.Tier,
                })
                .ToListAsync();

            return Ok(new
            {
                seasonId = season.Id,
                leagues = leagues
            });
        }

            [HttpGet("current")]
            public async Task<IActionResult> GetCurrentLeague(int gameSaveId, int seasonId, int leagueId)
            {
                // 🟢 1. Опитваме се да намерим активния сезон
                var activeSeason = await _context.Seasons
                    .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

                // 🟢 2. Ако няма активен сезон, fallback към подадения
                var targetSeasonId = activeSeason?.Id ?? seasonId;

                if (targetSeasonId == 0)
                    return Ok(new { exists = false, message = "❌ No active or provided season found." });

                // 🟢 3. Взимаме лигата (без Standings, за да не се мешат сезони)
                var league = await _context.Leagues
                    .Include(l => l.Template)
                    .Include(l => l.Country)
                    .FirstOrDefaultAsync(l =>
                        l.Id == leagueId &&
                        l.GameSaveId == gameSaveId &&
                        l.SeasonId == targetSeasonId);

                if (league == null)
                    return Ok(new { exists = false, message = "❌ League not found for active season." });

                var standings = await _context.LeagueStandings
                    .Include(s => s.Team)
                    .Where(s =>
                        s.GameSaveId == gameSaveId &&
                        s.LeagueId == leagueId &&
                        s.SeasonId == targetSeasonId)
                    .OrderBy(s => s.Ranking)
                    .Select(s => new
                    {
                        teamId = s.TeamId,
                        teamName = s.Team.Name,
                        teamLogo = s.Team.LogoFileName ?? "default_logo.png",
                        s.Ranking,
                        s.Matches,
                        s.Wins,
                        s.Draws,
                        s.Losses,
                        s.GoalsFor,
                        s.GoalsAgainst,
                        s.GoalDifference,
                        s.Points
                    })
                    .ToListAsync();

                // 🟢 5. Връщаме всичко
                var data = new
                {
                    exists = true,
                    id = league.Id,
                    name = league.Template.Name,
                    country = league.Country.Name,
                    seasonId = targetSeasonId,
                    standings
                };

                return Ok(data);
            }

        [HttpGet("{gameSaveId}/{leagueId}/top-scorers")]
        public async Task<IActionResult> GetTopScorersByLeague(int gameSaveId, int leagueId)
        {
            // Get current active season
            var season = await _context.Seasons
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (season == null)
                return NotFound("❌ No active season found.");

            // Get the league
            var league = await _context.Leagues
                .Include(l => l.Template)
                .Include(l => l.Country)
                .FirstOrDefaultAsync(l => l.Id == leagueId && l.GameSaveId == gameSaveId && l.SeasonId == season.Id);

            if (league == null)
                return NotFound("❌ League not found.");

            // Search for the competition associated with this league and season
            var competition = await _context.Competitions
                .FirstOrDefaultAsync(c => c.LeagueId == league.Id && c.SeasonId == season.Id);

            if (competition == null)
                return NotFound("❌ No competition found for this league.");

            // Get top scorers
            var scorers = await _context.PlayerCompetitionStats
                .Include(pcs => pcs.Player)
                    .ThenInclude(p => p.Team)
                .Where(pcs => pcs.CompetitionId == competition.Id && pcs.SeasonId == season.Id)
                .OrderByDescending(pcs => pcs.Goals)
                .ThenBy(pcs => pcs.Player.LastName)
                .Select(pcs => new
                {
                    id = pcs.PlayerId,
                    name = pcs.Player.FirstName + " " + pcs.Player.LastName,
                    teamName = pcs.Player.Team.Name,
                    goals = pcs.Goals,
                    matches = pcs.MatchesPlayed
                })
                .ToListAsync();

            if (!scorers.Any())
                return Ok(new List<object>()); 

            return Ok(scorers);
        }
    }
}
