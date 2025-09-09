using TheDugout.Models;

namespace TheDugout.Services.Training
{
    public interface ITrainingService
    {
        Task<List<TrainingResultDto>> RunTrainingSessionAsync(
            int gameSaveId,
            int teamId,
            int seasonId,
            DateTime date,
            List<PlayerTrainingAssignmentDto> assignments
        );
        Task<List<PlayerTrainingAssignmentDto>> AutoAssignAttributesAsync(int teamId, int gameSaveId);

    }

}
