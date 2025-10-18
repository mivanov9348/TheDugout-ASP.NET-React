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

        // 🟢 Взимане на всички лиги за сейв (ако ти трябва)
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
                .Include(l => l.Standings)
                    .ThenInclude(s => s.Team)
                .Where(l => l.GameSaveId == gameSaveId && l.SeasonId == season.Id)
                .ToListAsync();

            var result = leagues.Select(l => new
            {
                id = l.Id,
                name = l.Template.Name,
                country = l.Country.Name,
                standings = l.Standings
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
            });

            return Ok(new { seasonId = season.Id, leagues = result });
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentLeague(int gameSaveId, int seasonId)
        {
            var league = await _context.Leagues
                .Include(l => l.Template)
                .Include(l => l.Country)
                .Include(l => l.Standings)
                    .ThenInclude(s => s.Team)
                .FirstOrDefaultAsync(l => l.GameSaveId == gameSaveId && l.SeasonId == seasonId);

            if (league == null)
                return Ok(new { exists = false });

            var data = new
            {
                exists = true,
                id = league.Id,
                name = league.Template.Name,
                country = league.Country.Name,
                standings = league.Standings
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
            };

            return Ok(data);
        }


        [HttpGet("{gameSaveId}/{leagueId}/top-scorers")]
        public async Task<IActionResult> GetTopScorersByLeague(int gameSaveId, int leagueId)
        {
            // 🟢 Активен сезон
            var season = await _context.Seasons
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (season == null)
                return NotFound("❌ No active season found.");

            // 🟢 Лига
            var league = await _context.Leagues
                .Include(l => l.Template)
                .Include(l => l.Country)
                .FirstOrDefaultAsync(l => l.Id == leagueId && l.GameSaveId == gameSaveId && l.SeasonId == season.Id);

            if (league == null)
                return NotFound("❌ League not found.");

            // 🟢 Намираме competition (ако е нужно)
            var competition = await _context.Competitions
                .FirstOrDefaultAsync(c => c.LeagueId == league.Id && c.SeasonId == season.Id);

            if (competition == null)
                return NotFound("❌ No competition found for this league.");

            // 🟢 Всички голмайстори за конкретната лига
            var scorersQuery = _context.PlayerSeasonStats
                .Include(pss => pss.Player)
                    .ThenInclude(p => p.Team)
                .Where(pss => pss.SeasonId == season.Id);

            // Ако Player.Team има LeagueId – използваме това:
            scorersQuery = scorersQuery.Where(pss => pss.Player!.Team!.LeagueId == league.Id);

            // Ако не – ползваме CompetitionId (стария вариант)
            // scorersQuery = scorersQuery.Where(pss => pss.CompetitionId == competition.Id);

            var scorers = await scorersQuery
                .OrderByDescending(pss => pss.Goals)
                .ThenBy(pss => pss.Player!.LastName)
                .Select(pss => new
                {
                    id = pss.PlayerId,
                    name = $"{pss.Player!.FirstName} {pss.Player!.LastName}",
                    teamName = pss.Player!.Team!.Name,
                    goals = pss.Goals,
                    matches = pss.MatchesPlayed
                })
                .ToListAsync();

            return Ok(scorers);
        }


    }
}
