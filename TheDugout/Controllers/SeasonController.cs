namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using TheDugout.Data;
    using TheDugout.DTOs.Season;
    using TheDugout.Models.Enums;
    using TheDugout.Services.Competition.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class SeasonController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ICompetitionService _competitionService;
        private readonly ILogger<SeasonController> _logger;

        public SeasonController(DugoutDbContext context, ICompetitionService competitionService, ILogger<SeasonController> logger)
        {
            _context = context;
            _competitionService = competitionService;
            _logger = logger;
        }

        [HttpGet("{seasonId}/overview")]
        public async Task<IActionResult> GetSeasonOverview(int seasonId)
        {
            _logger.LogInformation("📅 GetSeasonOverview called for seasonId={SeasonId}", seasonId);

            var season = await _context.Seasons.FindAsync(seasonId);
            if (season == null)
                return NotFound();

            var allFinished = await _competitionService.AreAllCompetitionsFinishedAsync(seasonId);

            var results = await _context.CompetitionSeasonResults
                .Include(r => r.Competition).ThenInclude(c => c.League).ThenInclude(l => l.Template)
                .Include(r => r.Competition).ThenInclude(c => c.Cup).ThenInclude(cu => cu.Template)
                .Include(r => r.Competition).ThenInclude(c => c.EuropeanCup).ThenInclude(ec => ec.Template)
                .Include(r => r.ChampionTeam)
                .Include(r => r.RunnerUpTeam)
                .Include(r => r.PromotedTeams).ThenInclude(pt => pt.Team)
                .Include(r => r.RelegatedTeams).ThenInclude(rt => rt.Team)
                .Include(r => r.EuropeanQualifiedTeams).ThenInclude(eq => eq.Team)
                .Include(r => r.Awards).ThenInclude(a => a.Player)
                .Where(r => r.SeasonId == seasonId)
                .ToListAsync();

            var dto = new SeasonOverviewDto
            {
                SeasonId = seasonId,
                AllCompetitionsFinished = allFinished,
                Competitions = new List<CompetitionResultDto>()
            };

            foreach (var r in results)
            {
                var competitionDto = new CompetitionResultDto
                {
                    CompetitionId = r.CompetitionId ?? 0,
                    Name = _competitionService.GetCompetitionNameById(r.CompetitionId ?? 0),
                    Type = r.CompetitionType,
                    ChampionTeam = r.ChampionTeam?.Name,
                    RunnerUpTeam = r.RunnerUpTeam?.Name,
                    PromotedTeams = r.PromotedTeams.Select(t => t.Team.Name).ToList(),
                    RelegatedTeams = r.RelegatedTeams.Select(t => t.Team.Name).ToList(),
                    EuropeanQualifiedTeams = r.EuropeanQualifiedTeams.Select(t => t.Team.Name).ToList(),
                    Awards = r.Awards.Select(a => new AwardDto
                    {
                        PlayerName = $"{a.Player.FirstName} {a.Player.LastName}",
                        AwardType = a.AwardType,
                        Value = a.Value
                    }).ToList()
                };

                // 🆕 Ако това е лига — добави standings и top scorers
                if (r.CompetitionType == CompetitionTypeEnum.League && r.Competition?.League != null)
                {
                    int leagueId = r.Competition.League.Id;

                    // Класиране
                    var standings = await _context.LeagueStandings
                        .Include(s => s.Team)
                        .Where(s => s.LeagueId == leagueId && s.SeasonId == seasonId)
                        .OrderByDescending(s => s.Points)
                        .ThenByDescending(s => s.GoalDifference)
                        .ThenByDescending(s => s.GoalsFor)
                        .ToListAsync();

                    competitionDto.LeagueStandings = standings.Select(s => new LeagueStandingDto
                    {
                        TeamName = s.Team.Name,
                        Points = s.Points,
                        Wins = s.Wins,
                        Draws = s.Draws,
                        Losses = s.Losses,
                        GoalsFor = s.GoalsFor,
                        GoalsAgainst = s.GoalsAgainst,
                        GoalDifference = s.GoalDifference
                    }).ToList();

                    var topScorers = await _context.PlayerCompetitionStats
                        .Include(p => p.Player)
                        .Where(p => p.SeasonId == seasonId && p.CompetitionId == r.CompetitionId)
                        .OrderByDescending(p => p.Goals)
                        .ThenBy(p => p.MatchesPlayed)
                        .Take(10)
                        .ToListAsync();

                    competitionDto.TopScorers = topScorers.Select(ts => new TopScorerDto
                    {
                        PlayerName = $"{ts.Player.FirstName} {ts.Player.LastName}",
                        Goals = ts.Goals
                    }).ToList();
                }

                dto.Competitions.Add(competitionDto);
            }

            return Ok(dto);
        }

    }
}