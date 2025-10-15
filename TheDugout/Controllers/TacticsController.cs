using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.Team;
using TheDugout.Services.Team.Interfaces;

[ApiController]
[Route("api/tactics")]
[Authorize]
public class TacticsController : ControllerBase
{
    private readonly DugoutDbContext _context;
    private readonly ITeamPlanService _teamPlanService;

    public TacticsController(DugoutDbContext context, ITeamPlanService teamPlanService)
    {
        _context = context;
        _teamPlanService = teamPlanService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTactics()
    {
        var tactics = await _context.Tactics
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Defenders,
                t.Midfielders,
                t.Forwards
            })
            .ToListAsync();

        return Ok(tactics);
    }

    [HttpGet("{teamId:int}")]
    public async Task<IActionResult> GetTeamTactic(int teamId, [FromQuery] int saveId)
    {
        var tactic = await _teamPlanService.GetTeamTacticAsync(teamId, saveId);
        if (tactic == null)
            return NotFound("Този отбор няма зададена тактика.");

        return Ok(new
        {
            tacticId = tactic.TacticId,
            customName = tactic.CustomName,
            lineupJson = tactic.LineupJson
        });
    }

    [HttpPost("{teamId:int}")]
    public async Task<IActionResult> SetTeamTactic(int teamId, [FromBody] SetTacticRequest request)
    {
        if (request == null || request.TacticId <= 0)
            return BadRequest("Невалидни данни за тактика.");

        var tactic = await _teamPlanService.SetTeamTacticAsync(
            teamId,
            request.TacticId,
            request.CustomName,
            request.Lineup,
            request.Substitutes
        );

        return Ok(new
        {
            tactic.TacticId,
            tactic.CustomName,
            tactic.LineupJson,
            tactic.SubstitutesJson
        });
    }

}
