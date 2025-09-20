using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TheDugout.Data;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InboxController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public InboxController(DugoutDbContext context)
        {
            _context = context;
        }

        // GET: api/inbox/{gameSaveId}
        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] int gameSaveId)
        {
            var save = await _context.GameSaves
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == gameSaveId);

            if (save == null)
                return NotFound("No save found with this ID.");

            var messages = await _context.Messages
                        .Where(m => m.GameSaveId == gameSaveId)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new
                        {
                            m.Id,
                            m.Subject,
                            m.Body,
                            m.IsRead,
                            Date = m.CreatedAt
                        })
                        .ToListAsync();


            return Ok(messages);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, [FromQuery] int gameSaveId)
        {
            var message = await _context.Messages
                .Include(m => m.GameSave)
                .FirstOrDefaultAsync(m => m.Id == id && m.GameSave.Id == gameSaveId);

            if (message == null)
                return NotFound("Message not found.");

            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Message marked as read" });
        }
    }

}