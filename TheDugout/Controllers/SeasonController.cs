namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Season;
    using TheDugout.Services.Competition.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class SeasonController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly ICompetitionService _competitionService;
        public SeasonController(DugoutDbContext context, ICompetitionService competitionService)
        {
            _context = context;
            _competitionService = competitionService;
        }

        [HttpGet("{seasonId}/overview")]
        public async Task<IActionResult> GetSeasonOverview(int seasonId)
        {
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
                Competitions = results.Select(r => new CompetitionResultDto
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
                        PlayerName = a.Player.FirstName + " " + a.Player.LastName,
                        AwardType = a.AwardType,
                        Value = a.Value
                    }).ToList()
                }).ToList()
            };

            return Ok(dto);
        }
    }
}