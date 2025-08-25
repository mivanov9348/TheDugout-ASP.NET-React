using Microsoft.AspNetCore.Mvc;
using TheDugout.Models;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InboxController : ControllerBase
    {
        private static List<Message> Messages = new List<Message>
        {
            new Message { Id = 1, Subject = "Welcome to The Dugout!", Body = "Thank you for joining The Dugout. We're excited to have you on board.", isRead = false, Date = DateTime.Now.AddDays(-2) },
            new Message { Id = 2, Subject = "Your Weekly Update", Body = "Here's what happened in the world of football this week...", isRead = true, Date = DateTime.Now.AddDays(-1) },
        };

        [HttpGet]
        public IActionResult GetAllMessages()
        {
            return Ok(Messages);
        }

        [HttpGet("{id}")]
        public IActionResult GetMessageById(int id)
        {
            var message = Messages.FirstOrDefault(m => m.Id == id);
            if (message == null) return NotFound();
            return Ok(message);
        }

        [HttpPost("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var message = Messages.FirstOrDefault(m => m.Id == id);
            if (message == null) return NotFound();
            message.isRead = true;
            return NoContent();
        }
    }
}
