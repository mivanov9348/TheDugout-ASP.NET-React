using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;
using TheDugout.Services.CPUManager;
using TheDugout.Services.Team;
using TheDugout.Services.Training;
using TheDugout.Services.Transfer;

public class CpuManagerService : ICPUManagerService
{
    private readonly ITrainingService _trainingService;
    private readonly ITransferService _transferService;
    private readonly ITeamPlanService _teamPlanService;
    private readonly DugoutDbContext _context;
    private readonly ILogger<CpuManagerService> _logger;

    public CpuManagerService(
        ITrainingService trainingService,
        ITransferService transferService,
        ITeamPlanService teamPlanService,
        DugoutDbContext context,
        ILogger<CpuManagerService> logger)
    {
        _trainingService = trainingService;
        _transferService = transferService;
        _teamPlanService = teamPlanService;
        _context = context;
        _logger = logger;
    }

    public async Task RunDailyCpuLogicAsync(int gameSaveId, int seasonId, DateTime date, int? humanTeamId)
    {
        var season = await _context.Seasons
            .Include(s => s.Events)
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season == null) throw new Exception("Season not found.");

        var todayEvents = season.Events.Where(e => e.Date.Date == date.Date).ToList();

        if (!todayEvents.Any())
        {
            _logger.LogInformation("📅 {Date}: No Special Events → default TrainingDay", date);
            todayEvents.Add(new SeasonEvent { Type = SeasonEventType.TrainingDay, Date = date });
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
                            await _transferService.RunCpuTransfersAsync(gameSaveId, seasonId, date, team.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ CPU transfer logic error for team {TeamId}", team.Id);
                        }
                    }
                    break;

                case SeasonEventType.TrainingDay:
                    foreach (var team in cpuTeams)
                    {
                        try
                        {
                            await _trainingService.RunDailyCpuTrainingAsync(gameSaveId, seasonId, date, team.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ CPU training logic error for team {TeamId}", team.Id);
                        }
                    }
                    break;

                //case SeasonEventType.ChampionshipMatch:
                //case SeasonEventType.CupMatch:
                //case SeasonEventType.EuropeanMatch:
                //case SeasonEventType.FriendlyMatch:
                //    foreach (var team in cpuTeams)
                //        await _teamPlanService.AutoPickTacticAsync(team.Id, gameSaveId);
                //    break;

                default:
                    _logger.LogInformation("🤷 Няма логика за {EventType}", ev.Type);
                    break;
            }
        }
    }


}
