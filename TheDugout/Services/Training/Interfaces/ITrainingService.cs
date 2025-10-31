namespace TheDugout.Services.Training.Interfaces
{
    public interface ITrainingService
    {

        Task RunDailyTrainingForAllCpuTeamsAsync(int gameSaveId, int seasonId, DateTime date, int? humanTeamId);
        Task<List<AutoAssignResultDto>> AutoAssignAttributesAsync(int teamId, int gameSaveId);
        Task<List<TrainingResultDto>> RunTrainingSessionAsync(int gameSaveId, int teamId, List<PlayerTrainingAssignmentDto> assignments);
    }
}
