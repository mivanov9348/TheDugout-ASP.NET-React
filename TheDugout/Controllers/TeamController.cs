using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheDugout.Models;

[ApiController]
[Route("api/team")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTeam()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var team = await _teamService.GetMyTeamAsync(userId);

        if (team == null) return NotFound("Няма активен сейф или отбор.");
        return Ok(team);
    }

    [HttpGet("by-save/{saveId:int}")]
    public async Task<IActionResult> GetTeamBySave(int saveId)
    {
        var team = await _teamService.GetTeamBySaveAsync(saveId);
        if (team == null) return NotFound("Не е намерен отбор за този сейф");
        return Ok(team);
    }
}
