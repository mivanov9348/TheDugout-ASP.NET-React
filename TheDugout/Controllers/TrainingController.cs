using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Services.Training;

namespace TheDugout.Controllers
{
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


        [HttpPost("start")]
        public async Task<IActionResult> StartTraining([FromBody] TrainingRequestDto request)
        {
            if (request.Assignments == null || !request.Assignments.Any())
                return BadRequest("No training assignments provided.");

            try
            {
                var results = await _trainingService.RunTrainingSessionAsync(
                    request.GameSaveId,
                    request.TeamId,
                    request.SeasonId,
                    request.Date,
                    request.Assignments
                );

                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
