using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/team")]
    [Authorize] 
    public class TeamController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public TeamController(DugoutDbContext context)
        {
            _context = context;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyTeam()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);

            // Взимаме user с CurrentSaveId
            var user = await _context.Users
                .Include(u => u.CurrentSave)
                    .ThenInclude(gs => gs.UserTeam)
                        .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.CurrentSave == null || user.CurrentSave.UserTeam == null)
                return NotFound("Няма активен сейф или отбор.");

            return Ok(new
            {
                TeamId = user.CurrentSave.UserTeam.Id,
                TeamName = user.CurrentSave.UserTeam.Name,
                Players = user.CurrentSave.UserTeam.Players.Select(p => new
                {
                    p.Id,
                    Name = p.FirstName + " " + p.LastName,
                    Position = p.Position.Name,
                    Number = p.KitNumber,
                    Age = p.Age,
                    Country = p.Country != null ? p.Country.Name : ""
                })
            });
        }


        [HttpGet("by-save/{saveId:int}")]
        public async Task<IActionResult> GetTeamBySave(int saveId)
        {
            var save = await _context.GameSaves
                .Include(gs => gs.UserTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Country)
                .FirstOrDefaultAsync(gs => gs.Id == saveId);

            if (save == null || save.UserTeam == null)
                return NotFound("Не е намерен отбор за този сейф");

            return Ok(new
            {
                TeamId = save.UserTeam.Id,
                TeamName = save.UserTeam.Name,
                Players = save.UserTeam.Players.Select(p => new
                {
                    p.Id,
                    Name = p.FirstName + " " + p.LastName,
                    p.KitNumber,
                    Position = p.Position.Name,
                    Age = p.Age,
                    BirthDate = p.BirthDate.ToString("yyyy-MM-dd"),
                    Country = p.Country != null ? p.Country.Name : "",
                    HeightCm = p.HeightCm,
                    WeightKg = p.WeightKg,
                    Price = p.Price,
                    // можеш да върнеш и атрибути
                    Attributes = p.Attributes.Select(a => new
                    {
                        a.AttributeId,
                        a.Attribute.Name,
                        a.Value
                    }),
                    // пример за сезонна статистика
                    SeasonStats = p.SeasonStats.Select(s => new
                    {
                        s.SeasonId,
                        s.Goals,
                        s.Assists,
                        s.MatchesPlayed
                    })
                })
            });
        }


    }
}
