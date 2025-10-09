using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CupController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public CupController(DugoutDbContext context)
        {
            _context = context;
        }

        [HttpGet("{gameSaveId}/{seasonId}")]
        public async Task<IActionResult> GetCups(int gameSaveId, int seasonId)
        {
            var cups = await _context.Cups
                .Where(c => c.GameSaveId == gameSaveId && c.SeasonId == seasonId)
                .Include(c => c.Template)
                .Include(c => c.Country)
                .Include(c => c.Teams).ThenInclude(ct => ct.Team)
                .Include(c => c.Rounds).ThenInclude(r => r.Fixtures).ThenInclude(f => f.HomeTeam)
                .Include(c => c.Rounds).ThenInclude(r => r.Fixtures).ThenInclude(f => f.AwayTeam)
                .ToListAsync();

            if (!cups.Any())
                return NotFound($"No cups found for GameSave {gameSaveId}, Season {seasonId}");

            var result = new List<object>();

            foreach (var c in cups)
            {
                // Зареждаме всички fixtures от тази купа
                var fixtureIds = c.Rounds.SelectMany(r => r.Fixtures).Select(f => f.Id).ToList();

                // ЗАРЕЖДАНЕ НА МАЧОВЕТЕ за тези fixtures
                var matchIds = await _context.Matches
                    .Where(m => fixtureIds.Contains(m.FixtureId) && m.CompetitionId == c.CompetitionId)
                    .Select(m => m.Id)
                    .ToListAsync();

                // Зареждаме голмайсторите за всички тези мачове
                var goalScorersLookup = await _context.PlayerMatchStats
                    .Where(ps => matchIds.Contains(ps.MatchId) && ps.Goals > 0)
                    .Include(ps => ps.Player).ThenInclude(p => p.Team)
                    .ToListAsync();

                var totalGoals = c.Rounds
                    .SelectMany(r => r.Fixtures)
                    .Sum(f => (f.HomeTeamGoals ?? 0) + (f.AwayTeamGoals ?? 0));

                var cupPlayerStats = await _context.PlayerSeasonStats
                    .Where(p => p.CompetitionId == c.CompetitionId && p.SeasonId == seasonId)
                    .Include(p => p.Player).ThenInclude(pl => pl.Team)
                    .Select(p => new
                    {
                        PlayerId = p.PlayerId,
                        PlayerName = p.Player.FirstName,
                        TeamName = p.Player.Team != null ? p.Player.Team.Name : "Unknown",
                        Goals = p.Goals,
                        Matches = p.MatchesPlayed
                    })
                    .OrderByDescending(p => p.Goals)
                    .ThenBy(p => p.PlayerName)
                    .ToListAsync();

                result.Add(new
                {
                    c.Id,
                    c.TemplateId,
                    TemplateName = c.Template.Name,
                    c.GameSaveId,
                    c.SeasonId,
                    c.CountryId,
                    CountryName = c.Country.Name,
                    c.TeamsCount,
                    c.RoundsCount,
                    c.IsActive,
                    c.LogoFileName,
                    TotalGoals = totalGoals,
                    Teams = c.Teams.Select(t => new
                    {
                        t.TeamId,
                        TeamName = t.Team.Name
                    }),
                    Rounds = c.Rounds
                        .OrderBy(r => r.RoundNumber)
                        .Select(r => new
                        {
                            r.Id,
                            r.RoundNumber,
                            r.Name,
                            Fixtures = r.Fixtures.Select(f => {
                                // Намираме matchId за този fixture
                                var matchId = _context.Matches
                                    .FirstOrDefault(m => m.FixtureId == f.Id && m.CompetitionId == c.CompetitionId)?.Id ?? 0;

                                return new
                                {
                                    f.Id,
                                    f.Round,
                                    f.Date,
                                    f.Status,
                                    HomeTeam = new { f.HomeTeamId, f.HomeTeam.Name, LogoFileName = f.HomeTeam.LogoFileName },
                                    AwayTeam = new { f.AwayTeamId, f.AwayTeam.Name, LogoFileName = f.AwayTeam.LogoFileName },
                                    f.HomeTeamGoals,
                                    f.AwayTeamGoals,
                                    MatchId = matchId, 
                                    GoalScorers = goalScorersLookup
                                        .Where(g => g.MatchId == matchId)
                                        .Select(g => new
                                        {
                                            g.Player.FirstName,
                                            TeamName = g.Player.Team.Name != null ? g.Player.Team.Name : "Unknown",
                                            g.Goals
                                        })
                                        .ToList()
                                };
                            })
                        }),
                    PlayerStats = cupPlayerStats
                });
            }

            return Ok(result);
        }


        [HttpGet("{cupId}/player-stats")]
        public async Task<IActionResult> GetCupPlayerStats(int cupId)
        {
            // намираме купата
            var cup = await _context.Cups
                .FirstOrDefaultAsync(c => c.Id == cupId);

            if (cup == null)
                return NotFound($"Cup with id {cupId} not found");

            // намираме всички competitions, които са част от тази купа
            var competitionIds = await _context.Competitions
                .Where(comp => comp.CupId == cupId)
                .Select(comp => comp.Id)
                .ToListAsync();

            if (!competitionIds.Any())
                return Ok(new List<object>()); // няма competitions => няма статистики

            // взимаме всички статистики от тези competitions
            var playerStats = await _context.PlayerMatchStats
                .Where(p => competitionIds.Contains(p.CompetitionId))
                .Include(p => p.Player)
                .ThenInclude(player => player.Team)
                .GroupBy(p => new
                {
                    p.PlayerId,
                    p.Player.FirstName,
                    TeamName = p.Player.Team != null ? p.Player.Team.Name : "Unknown"
                })
                .Select(g => new
                {
                    Id = g.Key.PlayerId,
                    Name = g.Key.FirstName,
                    TeamName = g.Key.TeamName,
                    Goals = g.Sum(x => x.Goals),
                    Matches = g.Select(x => x.MatchId).Distinct().Count()
                })
                .OrderByDescending(g => g.Goals)
                .ThenBy(g => g.Name)
                .ToListAsync();

            return Ok(playerStats);
        }

    }
}

