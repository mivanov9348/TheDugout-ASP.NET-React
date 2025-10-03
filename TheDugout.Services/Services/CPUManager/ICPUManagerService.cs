namespace TheDugout.Services.CPUManager
{
    public interface ICPUManagerService
    {
        Task RunDailyCpuLogicAsync(
            int gameSaveId,
            int seasonId,
            DateTime date,
            int? humanTeamId,
            Func<string, Task>? progress = null);
    }
}
