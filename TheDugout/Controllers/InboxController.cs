using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using TheDugout.Data;
using TheDugout.Models.Seasons;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonEventsController : ControllerBase
    {
        private readonly DugoutDbContext _context;

        public SeasonEventsController(DugoutDbContext context)
        {
            _context = context;
        }

        // GET: api/SeasonEvents/1
        // Връща всички евенти за конкретен сезон
        [HttpGet("{seasonId}")]
        public async Task<IActionResult> GetEventsBySeason(int seasonId)
        {
            var events = await _context.SeasonEvents
                .Where(e => e.SeasonId == seasonId)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/SeasonEvents/details/5
        // Връща конкретен евент по Id
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var seasonEvent = await _context.SeasonEvents
                .FirstOrDefaultAsync(e => e.Id == id);

            if (seasonEvent == null)
                return NotFound();

            return Ok(seasonEvent);
        }

        // POST: api/SeasonEvents
        // Създава нов евент за сезона
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] SeasonEvent seasonEvent)
        {
            if (seasonEvent == null)
                return BadRequest("Invalid event data");

            // проверка дали сезонът съществува
            var seasonExists = await _context.Seasons.AnyAsync(s => s.Id == seasonEvent.SeasonId);
            if (!seasonExists)
                return BadRequest($"Season with Id {seasonEvent.SeasonId} does not exist.");

            _context.SeasonEvents.Add(seasonEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = seasonEvent.Id }, seasonEvent);
        }

        // PUT: api/SeasonEvents/5
        // Ъпдейт на съществуващ евент
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] SeasonEvent updatedEvent)
        {
            if (id != updatedEvent.Id)
                return BadRequest("Event ID mismatch");

            var existing = await _context.SeasonEvents.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Date = updatedEvent.Date;
            existing.Type = updatedEvent.Type;
            existing.Description = updatedEvent.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/SeasonEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var existing = await _context.SeasonEvents.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.SeasonEvents.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
