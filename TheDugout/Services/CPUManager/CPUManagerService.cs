namespace TheDugout.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Seasons;
    using TheDugout.Services.CPUManager.Interfaces;
    using TheDugout.Services.Team.Interfaces;
    using TheDugout.Services.Training.Interfaces;
    using TheDugout.Services.Transfer;

    public class CpuManagerService : ICPUManagerService
    {
        private readonly ITrainingService _trainingService;
        private readonly ITeamPlanService _teamPlanService;
        private readonly ICPUTransferService _transferService;
        private readonly DugoutDbContext _context;
        private readonly Random _random;
        private readonly ILogger<CpuManagerService> _logger;

        public CpuManagerService(
            ITrainingService trainingService,
            ITeamPlanService teamPlanService,
            ICPUTransferService transferService,
            DugoutDbContext context,
            ILogger<CpuManagerService> logger)
        {
            _trainingService = trainingService;
            _teamPlanService = teamPlanService;
            _transferService = transferService;
            _random = new Random();
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
                        {
                            var activeTeams = cpuTeams
                                .Where(t => t.Balance > 50_000 && _random.NextDouble() > 0.3) // 30% шанс да пропусне пазара
                                .ToList();

                            foreach (var team in activeTeams)
                            {
                                try
                                {
                                    if (progress != null) await progress($"🔄 {team.Name} is analyzing market...");
                                    await _transferService.RunCpuTransfersAsync(gameSaveId, seasonId, date, team.Id);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "❌ Transfer error for {TeamId}", team.Id);
                                }
                            }
                            break;
                        }



                    case SeasonEventType.TrainingDay:
                        try
                        {
                            if (progress != null) await progress("💪 Провежда се тренировка за всички CPU отбори...");

                            await _trainingService.RunDailyTrainingForAllCpuTeamsAsync(gameSaveId, seasonId, date, humanTeamId);

                            if (progress != null) await progress("✅ The training is over!");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Критична грешка при масовата CPU тренировка");
                            if (progress != null) await progress("❌ Възникна грешка по време на тренировката");
                        }
                        break;

                    default:
                        var defMsg = $"🤷 No logic implemented for event {ev.Type}";
                        _logger.LogInformation(defMsg);
                        if (progress != null) await progress(defMsg);
                        break;
                }
            }
        }
    }
}