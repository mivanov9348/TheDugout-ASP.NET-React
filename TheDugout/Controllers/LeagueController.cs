using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TheDugout.Data;
using TheDugout.Models.Leagues;
using TheDugout.Models.Matches;
using TheDugout.Models.Teams;
using TheDugout.Models.Seasons;

namespace TheDugout.Controllers
{
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
            try
            {
                // 1️⃣ Намираме активния сезон за този сейв
                var season = await _context.Seasons
                    .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

                if (season == null)
                    return NotFound($"❌ No active season found for GameSave {gameSaveId}");

                // 2️⃣ Взимаме всички лиги за този сейв и сезон
                var leagues = await _context.Leagues
                    .Include(l => l.Template)
                    .Include(l => l.Country)
                    .Include(l => l.Standings)
                        .ThenInclude(ls => ls.Team)
                            .ThenInclude(t => t.Template)
                    .Where(l => l.GameSaveId == gameSaveId && l.SeasonId == season.Id)
                    .ToListAsync();

                Console.WriteLine($"✅ Found {leagues.Count} leagues for season {season.Id}");

                // 3️⃣ Филтрираме Standings-ите за точния сезон и сейв
                var filteredLeagues = leagues.Select(l => new
                {
                    Id = l.Id,
                    Name = l.Template?.Name ?? "Unknown League",
                    Country = l.Country?.Name ?? "Unknown Country",
                    Tier = l.Tier,
                    PromotionSpots = l.PromotionSpots,
                    RelegationSpots = l.RelegationSpots,
                    TeamsCount = l.TeamsCount,
                    Standings = l.Standings
                        .Where(s => s.SeasonId == season.Id && s.GameSaveId == gameSaveId)
                        .OrderBy(s => s.Ranking)
                        .Select(s => new
                        {
                            TeamId = s.TeamId,
                            TeamName = s.Team?.Name ?? "Unknown Team",
                            TeamAbbr = s.Team?.Abbreviation ?? "",
                            TeamLogo = s.Team?.LogoFileName ?? "default_logo.png",
                            Ranking = s.Ranking,
                            Points = s.Points,
                            Matches = s.Matches,
                            Wins = s.Wins,
                            Draws = s.Draws,
                            Losses = s.Losses,
                            GoalsFor = s.GoalsFor,
                            GoalsAgainst = s.GoalsAgainst,
                            GoalDifference = s.GoalDifference
                        })
                        .ToList()
                }).ToList();

                var jsonDebug = System.Text.Json.JsonSerializer.Serialize(
                           filteredLeagues,
                           new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                       );

                Console.WriteLine("📊 Filtered Leagues:");
                Console.WriteLine(jsonDebug);

                // 4️⃣ Връщаме SeasonId + списък с лиги и класирания
                return Ok(new
                {
                    SeasonId = season.Id,
                    Leagues = filteredLeagues
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching leagues: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }


    }
}