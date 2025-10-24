namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Facility;
    using TheDugout.Services.Facilities;

    [ApiController]
    [Route("api/[controller]")]
    public class FacilityController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly IStadiumService _stadiumService;
        private readonly ITrainingFacilitiesService _trainingService;
        private readonly IYouthAcademyService _academyService;

        public FacilityController(
            DugoutDbContext context,
            IStadiumService stadiumService,
            ITrainingFacilitiesService trainingService,
            IYouthAcademyService academyService)
        {
            _context = context;
            _stadiumService = stadiumService;
            _trainingService = trainingService;
            _academyService = academyService;
        }

        // GET: api/facility/team/5
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetFacilities(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Stadium)
                .Include(t => t.TrainingFacility)
                .Include(t => t.YouthAcademy)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                return NotFound($"No team with id {teamId}");
            }

            try
            {
                var dto = new FacilitiesDto
                {
                    Stadium = team.Stadium == null ? null : new StadiumDto
                    {
                        Level = team.Stadium.Level,
                        Capacity = team.Stadium.Capacity,
                        TicketPrice = team.Stadium.TicketPrice,
                        UpgradeCost = _stadiumService.GetNextUpgradeCost(team.Stadium.Level)
                    },
                    TrainingFacility = team.TrainingFacility == null ? null : new TrainingFacilityDto
                    {
                        Level = team.TrainingFacility.Level,
                        TrainingQuality = team.TrainingFacility.TrainingQuality,
                        UpgradeCost = _trainingService.GetNextUpgradeCost(team.TrainingFacility.Level)
                    },
                    YouthAcademy = team.YouthAcademy == null ? null : new YouthAcademyDto
                    {
                        Level = team.YouthAcademy.Level,
                        UpgradeCost = _academyService.GetNextUpgradeCost(team.YouthAcademy.Level)
                    }
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error mapping facilities: {ex.Message}");
            }
        }

        // POST: api/facility/stadium/upgrade/5
        [HttpPost("stadium/upgrade/{teamId}")]
        public async Task<IActionResult> UpgradeStadium(int teamId)
        {
            var result = await _stadiumService.UpgradeStadiumAsync(teamId);
            if (!result) return BadRequest("Not enough funds or invalid upgrade.");
            return Ok(new { Message = "Stadium upgraded successfully" });
        }

        // POST: api/facility/training/upgrade/5
        [HttpPost("training/upgrade/{teamId}")]
        public async Task<IActionResult> UpgradeTraining(int teamId)
        {
            var result = await _trainingService.UpgradeTrainingFacilityAsync(teamId);
            if (!result) return BadRequest("Not enough funds or invalid upgrade.");
            return Ok(new { Message = "Training Facility upgraded successfully" });
        }

        // POST: api/facility/academy/upgrade/5
        [HttpPost("academy/upgrade/{teamId}")]
        public async Task<IActionResult> UpgradeAcademy(int teamId)
        {
            var result = await _academyService.UpgradeYouthAcademyAsync(teamId);
            if (!result) return BadRequest("Not enough funds or invalid upgrade.");
            return Ok(new { Message = "Youth Academy upgraded successfully" });
        }
    }
}
