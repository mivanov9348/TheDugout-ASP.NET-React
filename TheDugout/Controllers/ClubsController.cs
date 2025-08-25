using Microsoft.AspNetCore.Mvc;

namespace TheDugout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubsController : ControllerBase
    {
        
        private static readonly List<object> Clubs = new()
        {
            new { Id = 1, Name = "FC Barcelona", Country = "Spain" },
            new { Id = 2, Name = "Manchester United", Country = "England" },
            new { Id = 3, Name = "Bayern Munich", Country = "Germany" },
                        new { Id = 4, Name = "ss Munich", Country = "ss" }

        };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Clubs);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var club = Clubs.FirstOrDefault(c => (int)c.GetType().GetProperty("Id")!.GetValue(c)! == id);
            if (club == null) return NotFound();
            return Ok(club);
        }
    }
}
