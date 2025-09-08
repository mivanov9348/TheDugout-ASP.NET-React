using TheDugout.Models;

namespace TheDugout.Services.Team
{
    public interface ITeamPlanService
    {
        Dictionary<string, int> GetDefaultRosterPlan();
        Task<TeamTactic?> GetTeamTacticAsync(int teamId, int gameSaveId);
        Task<TeamTactic> SetTeamTacticAsync(int teamId, int tacticId, string? customName, Dictionary<string, string?> lineup);
        Task InitializeDefaultTacticsAsync(GameSave gameSave);

    }


}
