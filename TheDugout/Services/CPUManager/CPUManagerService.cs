using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Enums;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;
using TheDugout.Services.CPUManager;
using TheDugout.Services.Team.Interfaces;
using TheDugout.Services.Training;
using TheDugout.Services.Transfer;

public class CpuManagerService : ICPUManagerService
{
    private readonly ITrainingService _trainingService;
    private readonly ITeamPlanService _teamPlanService;
    private readonly DugoutDbContext _context;
    private readonly ILogger<CpuManagerService> _logger;

    public CpuManagerService(
        ITrainingService trainingService,
        ITeamPlanService teamPlanService,
        DugoutDbContext context,
        ILogger<CpuManagerService> logger)
    {
        _trainingService = trainingService;
        _teamPlanService = teamPlanService;
        _context = context;
        _logger = logger;
    }

    public async Task RunDailyCpuLogicAsync(
        int gameSaveId,
        int seasonId,
        DateTime date,
        int? humanTeamId,
        Func<string, Task>? progress = null)
    {
        var season = await _context.Seasons
            .Include(s => s.Events)
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season == null) throw new Exception("Season not found.");

        var todayEvents = season.Events.Where(e => e.Date.Date == date.Date).ToList();

        if (!todayEvents.Any())
        {
            var msg = $"📅 {date:dd/MM/yyyy}: No Special Events → default TrainingDay";
            _logger.LogInformation(msg);
            if (progress != null) await progress(msg);

            todayEvents.Add(new SeasonEvent { Type = SeasonEventType.TrainingDay, Date = date, GameSaveId = gameSaveId });
        }

        var cpuTeams = await _context.Teams
            .Where(t => t.GameSaveId == gameSaveId && (humanTeamId == null || t.Id != humanTeamId))
            .ToListAsync();

        foreach (var ev in todayEvents)
        {
            switch (ev.Type)
            {
                case SeasonEventType.TransferWindow:
                    foreach (var team in cpuTeams)
                    {
                        try
                        {
                            if (progress != null) await progress($"🔄 CPU team {team.Name} is checking transfers...");
                            //await _transferService.RunCpuTransfersAsync(gameSaveId, seasonId, date, team.Id);
                            if (progress != null) await progress($"✅ CPU team {team.Name} finished transfer checks");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ CPU transfer logic error for team {TeamId}", team.Id);
                            if (progress != null) await progress($"❌ Transfer error for team {team.Name}");
                        }
                    }
                    break;

                case SeasonEventType.TrainingDay:
                    try
                    {
                        // Message before starting the bulk operation
                        if (progress != null) await progress("💪 Провежда се тренировка за всички CPU отбори...");

                        // All CPU teams train in one go
                        await _trainingService.RunDailyTrainingForAllCpuTeamsAsync(gameSaveId, seasonId, date, humanTeamId);

                        if (progress != null) await progress("✅ The training is over!");
                    }
                    catch (Exception ex)
                    {
                        // Error handling for the bulk operation
                        _logger.LogError(ex, "❌ Критична грешка при масовата CPU тренировка");
                        if (progress != null) await progress("❌ Възникна грешка по време на тренировката");
                    }
                    break;

                //case SeasonEventType.ChampionshipMatch:
                //case SeasonEventType.CupMatch:
                //case SeasonEventType.EuropeanMatch:
                //case SeasonEventType.FriendlyMatch:
                //    foreach (var team in cpuTeams)
                //    {
                //        if (progress != null) await progress($"⚽ CPU team {team.Name} is auto-picking lineup...");
                //        await _teamPlanService.AutoPickTacticAsync(team.Id, gameSaveId);
                //        if (progress != null) await progress($"✅ CPU team {team.Name} finished tactic selection");
                //    }
                //    break;

                default:
                    var defMsg = $"🤷 No logic implemented for event {ev.Type}";
                    _logger.LogInformation(defMsg);
                    if (progress != null) await progress(defMsg);
                    break;
            }
        }
    }
}
