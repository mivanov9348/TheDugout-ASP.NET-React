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
                .Include(c => c.Teams)
                    .ThenInclude(ct => ct.Team)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                .ToListAsync();

            if (!cups.Any())
                return NotFound($"No cups found for GameSave {gameSaveId}, Season {seasonId}");

            var result = cups.Select(c => new
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
                Teams = c.Teams.Select(t => new
                {
                    t.TeamId,
                    TeamName = t.Team.Name
                }),
                Rounds = c.Rounds.OrderBy(r => r.RoundNumber).Select(r => new
                {
                    r.Id,
                    r.RoundNumber,
                    r.Name,
                    Fixtures = r.Fixtures.Select(f => new
                    {
                        f.Id,
                        f.Round,
                        f.Date,
                        f.Status,
                        HomeTeam = new { f.HomeTeamId, f.HomeTeam.Name },
                        AwayTeam = new { f.AwayTeamId, f.AwayTeam.Name },
                        f.HomeTeamGoals,
                        f.AwayTeamGoals
                    })
                })
            });

            return Ok(result);
        }
    }
}
