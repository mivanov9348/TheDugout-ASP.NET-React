using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services;
using TheDugout.Services.EuropeanCup;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EuropeanCupController : ControllerBase
    {
        private readonly IEuropeanCupService _europeanCupService;
        private readonly DugoutDbContext _context;

        public EuropeanCupController(IEuropeanCupService europeanCupService, DugoutDbContext context)
        {
            _europeanCupService = europeanCupService;
            _context = context;
        }

        // GET: api/EuropeanCup/current?gameSaveId=5&seasonId=3
        [HttpGet("current")]
public async Task<IActionResult> GetCurrentEuropeanCup(int gameSaveId, int seasonId)
{
            var cup = await _context.Set<EuropeanCup>()
            .Include(c => c.Template)
            .Include(c => c.Teams)
                .ThenInclude(ct => ct.Team)
            .Include(c => c.Phases)
                .ThenInclude(p => p.Fixtures)
                    .ThenInclude(f => f.HomeTeam)
            .Include(c => c.Phases)
                .ThenInclude(p => p.Fixtures)
                    .ThenInclude(f => f.AwayTeam)
            .Include(c => c.Standings) // 🔹 добавяме standings
                .ThenInclude(s => s.Team)
            .FirstOrDefaultAsync(c => c.GameSaveId == gameSaveId && c.SeasonId == seasonId);

            if (cup == null)
        return NotFound(new { message = "Няма Европейски турнир за този сезон." });

            return Ok(new
            {
                cup.Id,
                cup.Template.Name,
                Teams = cup.Teams.Select(t => new
                {
                    t.Team.Id,
                    t.Team.Name,
                    t.Team.Abbreviation,
                    t.Team.LogoFileName
                }),
                Standings = cup.Standings
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .Select(s => new
                {
                    s.TeamId,
                    s.Team.Name,
                    s.Points,
                    s.Matches,
                    s.Wins,
                    s.Draws,
                    s.Losses,
                    s.GoalsFor,
                    s.GoalsAgainst,
                    s.GoalDifference,
                    s.Ranking
                }),
                Fixtures = cup.Phases
                .SelectMany(p => p.Fixtures)
                .GroupBy(f => f.Round) 
                .Select(g => new
                {
                    Round = g.Key,
                    Matches = g.Select(f => new
                    {
                        f.Id,
                        HomeTeam = f.HomeTeam?.Name,
                        AwayTeam = f.AwayTeam?.Name,
                        f.HomeTeamGoals,
                        f.AwayTeamGoals,
                        f.Date,
                        f.Status
                    })
                })
            });

        }

    }
}
