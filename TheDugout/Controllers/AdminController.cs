namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using TheDugout.Data;
    using TheDugout.Models.Game;

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public AdminController(DugoutDbContext context)
        {
            _context = context;
        }
        private async Task<bool> IsAdmin()
        {
            var emailClaim = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (emailClaim == null) return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
            return user?.IsAdmin == true;
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            if (!await IsAdmin())
                return Unauthorized("No Access");

            var settings = await _context.GameSettings.ToListAsync();
            return Ok(settings);
        }


        [HttpPut("settings/update")]
        public async Task<IActionResult> UpdateSettings([FromBody] List<SettingUpdateDto> updates)
        {
            if (!await IsAdmin())
                return Unauthorized("No Acess!");

            foreach (var update in updates)
            {
                var setting = await _context.GameSettings.FirstOrDefaultAsync(s => s.Id == update.Id);
                if (setting == null) continue;

                setting.Value = update.Value;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Settings are updated!" });
        }
    }
    public class SettingUpdateDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }

}