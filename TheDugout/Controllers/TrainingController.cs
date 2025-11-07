namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Training;
    using TheDugout.Services.Training;
    using TheDugout.Services.Training.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrainingController : ControllerBase
    {
        private readonly ITrainingService _trainingService;

        public TrainingController(ITrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet("auto-assign/{teamId:int}/{gameSaveId:int}")]
        public async Task<IActionResult> AutoAssignAttributes(int teamId, int gameSaveId)
        {
            try
            {
                if (teamId <= 0 || gameSaveId <= 0)
                {
                    return BadRequest(new { message = "Невалидни параметри", teamId, gameSaveId });
                }

                Console.WriteLine($"➡ AutoAssign called with teamId={teamId}, gameSaveId={gameSaveId}");

                var assignments = await _trainingService.AutoAssignAttributesAsync(teamId, gameSaveId);

                if (assignments == null || !assignments.Any())
                {
                    return BadRequest(new { message = "Няма предложения за трениране", teamId, gameSaveId });
                }

                Console.WriteLine($"✅ AutoAssign returned {assignments.Count()} assignments");
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AutoAssign error: " + ex);
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveTraining([FromBody] TrainingRequestDto request)
        {
            try
            {
                await _trainingService.SaveTrainingAsync(request);
                return Ok(new { message = "Training plan saved successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error while saving training." });
            }
        }

    }
}
