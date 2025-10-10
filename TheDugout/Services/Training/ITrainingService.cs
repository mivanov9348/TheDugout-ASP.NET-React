using TheDugout.Models;

namespace TheDugout.Services.Training
{
    public interface ITrainingService
    {

        Task RunDailyTrainingForAllCpuTeamsAsync(int gameSaveId, int seasonId, DateTime date, int? humanTeamId);
        Task<List<AutoAssignResultDto>> AutoAssignAttributesAsync(int teamId, int gameSaveId);
        Task<List<TrainingResultDto>> RunTrainingSessionAsync(int gameSaveId, int teamId, int seasonId, DateTime date, List<PlayerTrainingAssignmentDto> assignments);
    }
}
