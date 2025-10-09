using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.Player;
using TheDugout.Models.Competitions;
using TheDugout.Models.Matches;
using TheDugout.Models.Players;
using TheDugout.Services.EuropeanCup;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EuropeanCupController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly IEuropeanCupStandingService _standingService;
        private readonly IEurocupFixturesService _fixturesService;
        private readonly IEurocupKnockoutService _knockoutService;

        public EuropeanCupController(
            DugoutDbContext context,
            IEuropeanCupStandingService standingService,
            IEurocupFixturesService fixturesService,
            IEurocupKnockoutService knockoutService)
        {
            _context = context;
            _standingService = standingService;
            _fixturesService = fixturesService;
            _knockoutService = knockoutService;
        }

        // GET: api/EuropeanCup/current?gameSaveId=5&seasonId=3
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentEuropeanCup(int gameSaveId, int seasonId)
        {
            var cup = await _context.Set<EuropeanCup>()
                .Include(c => c.Template)
                .Include(c => c.Teams).ThenInclude(ct => ct.Team)
                .Include(c => c.Standings).ThenInclude(s => s.Team)
                .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate)
                .Include(c => c.Competition) // ✅ добавяме Competition
                .FirstOrDefaultAsync(c => c.GameSaveId == gameSaveId && c.SeasonId == seasonId);

            if (cup == null)
                return Ok(new { exists = false });

            var allFixtures = await _fixturesService.GetAllFixturesForCupAsync(cup.Id);
            var groupFixtures = await _fixturesService.GetGroupFixturesAsync(cup.Id);
            var knockoutFixtures = await _knockoutService.GetKnockoutFixturesAsync(cup.Id);
            var sortedStandings = await _standingService.GetSortedStandingsAsync(cup.Id);

            // --- ТУК СЕ ПРОВЕРЯВАМЕ CompetitionId ---
            int? competitionId = cup.CompetitionId;
            if (competitionId == null)
            {
                var competition = await _context.Competitions
                    .FirstOrDefaultAsync(c => c.EuropeanCup != null && c.EuropeanCup.Id == cup.Id);
                competitionId = competition?.Id;
            }

            var goalscorersData = await _context.PlayerMatchStats
       .Include(pms => pms.Player)
           .ThenInclude(p => p.Team)
       .Include(pms => pms.Match)
           .ThenInclude(m => m.Fixture)
       .Where(pms => pms.Goals > 0 &&
                    pms.Match.Competition.Id == competitionId &&
                    pms.Match.Status == MatchStatus.Played)
       .Select(pms => new
       {
           MatchId = pms.Match.FixtureId,
           PlayerName = pms.Player.FirstName + " " + pms.Player.LastName,
           TeamId = pms.Player.TeamId,
           Goals = pms.Goals,
           Minute = pms.Match.CurrentMinute // или специално поле за минута на гол, ако имате такова
       })
       .ToListAsync();

            // Групиране по мачове
            var goalsByMatch = goalscorersData
                .GroupBy(g => g.MatchId)
                .ToDictionary(g => g.Key, g => g.ToList());

            List<PlayerStatsDTO> playerStats = new();

            if (competitionId != null)
            {
                playerStats = await _context.PlayerSeasonStats
                    .Include(ps => ps.Player)
                        .ThenInclude(p => p.Team)
                    .Where(ps => ps.CompetitionId == competitionId)
                    .Select(ps => new PlayerStatsDTO
                    {
                        Id = ps.Player.Id,
                        Name = ps.Player.FirstName + " " + ps.Player.LastName,
                        TeamName = ps.Player.Team.Name,
                        Goals = ps.Goals,
                        Matches = ps.MatchesPlayed
                    })
                    .OrderByDescending(p => p.Goals)
                    .ThenBy(p => p.Name)
                    .ToListAsync();
            }

            var result = new
            {
                exists = true,
                id = cup.Id,
                name = cup.Template.Name,
                logoFileName = cup.LogoFileName,
                teams = cup.Teams.Select(t => new
                {
                    id = t.Team.Id,
                    name = t.Team.Name,
                    abbreviation = t.Team.Abbreviation,
                    logoFileName = t.Team.LogoFileName
                }),
                standings = sortedStandings,
                fixtures = allFixtures
                    .GroupBy(f => f.Round)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        round = g.Key,
                        matches = g.Select(f => new
                        {
                            id = f.Id,
                            homeTeam = f.HomeTeam == null ? null : new
                            {
                                id = f.HomeTeam.Id,
                                name = f.HomeTeam.Name,
                                logoFileName = f.HomeTeam.LogoFileName
                            },
                            awayTeam = f.AwayTeam == null ? null : new
                            {
                                id = f.AwayTeam.Id,
                                name = f.AwayTeam.Name,
                                logoFileName = f.AwayTeam.LogoFileName
                            },
                            homeTeamGoals = f.HomeTeamGoals,
                            awayTeamGoals = f.AwayTeamGoals,
                            date = f.Date,
                            status = f.Status,
                        })
                    }),
                groupFixtures = groupFixtures,
                knockoutFixtures = knockoutFixtures,
                playerStats = playerStats
            };

            return Ok(result);
        }


    }
}