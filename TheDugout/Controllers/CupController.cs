namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;

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
                // Всички fixtures от тази купа
                var fixtureIds = c.Rounds.SelectMany(r => r.Fixtures).Select(f => f.Id).ToList();

                // Зареждаме всички мачове (включително дузпите)
                var matches = await _context.Matches
    .Where(m => fixtureIds.Contains(m.FixtureId))
    .Include(m => m.Penalties)
    .ToListAsync();


                // Зареждаме голмайсторите за всички тези мачове
                var matchIds = matches.Select(m => m.Id).ToList();

                var goalScorersLookup = await _context.PlayerMatchStats
                    .Where(ps => matchIds.Contains(ps.MatchId) && ps.Goals > 0)
                    .Include(ps => ps.Player).ThenInclude(p => p.Team)
                    .ToListAsync();

                // Общо голове в турнира
                var totalGoals = c.Rounds
                    .SelectMany(r => r.Fixtures)
                    .Sum(f => (f.HomeTeamGoals ?? 0) + (f.AwayTeamGoals ?? 0));

                // Играческа статистика за турнира
                var cupPlayerStats = await _context.PlayerSeasonStats
                    .Where(p => p.CompetitionId == c.CompetitionId && p.SeasonId == seasonId)
                    .Include(p => p.Player).ThenInclude(pl => pl.Team)
                    .Select(p => new
                    {
                        PlayerId = p.PlayerId,
                        PlayerName = p.Player.FirstName + " " + p.Player.LastName,
                        TeamName = p.Player.Team.Name ?? "Unknown",
                        Goals = p.Goals,
                        Matches = p.MatchesPlayed
                    })
                    .OrderByDescending(p => p.Goals)
                    .ThenBy(p => p.PlayerName)
                    .ToListAsync();

                // Сглобяваме финалния резултат
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
                            Fixtures = r.Fixtures.Select(f =>
                            {
                                var match = matches.FirstOrDefault(m => m.FixtureId == f.Id);

                                string? penaltiesResult = null;
                                if (match != null && match.Penalties.Any())
                                {
                                    var homePenalties = match.Penalties.Count(p => p.TeamId == f.HomeTeamId && p.IsScored);
                                    var awayPenalties = match.Penalties.Count(p => p.TeamId == f.AwayTeamId && p.IsScored);
                                    penaltiesResult = $"({homePenalties} - {awayPenalties} п.)";
                                }

                                return new
                                {
                                    f.Id,
                                    f.Round,
                                    f.Date,
                                    f.Status,
                                    HomeTeam = new
                                    {
                                        f.HomeTeamId,
                                        f.HomeTeam.Name,
                                        LogoFileName = f.HomeTeam.LogoFileName
                                    },
                                    AwayTeam = new
                                    {
                                        f.AwayTeamId,
                                        f.AwayTeam.Name,
                                        LogoFileName = f.AwayTeam.LogoFileName
                                    },
                                    f.HomeTeamGoals,
                                    f.AwayTeamGoals,
                                    MatchId = match?.Id,
                                    PenaltiesResult = penaltiesResult,
                                    GoalScorers = goalScorersLookup
                                        .Where(g => g.MatchId == match?.Id)
                                        .Select(g => new
                                        {
                                            g.Player.FirstName,
                                            TeamName = g.Player.Team.Name ?? "Unknown",
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
                    p.Player.LastName,
                    TeamName = p.Player.Team != null ? p.Player.Team.Name : "Unknown"
                })
                .Select(g => new
                {
                    Id = g.Key.PlayerId,
                    Name = g.Key.FirstName + " " + g.Key.LastName,
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

