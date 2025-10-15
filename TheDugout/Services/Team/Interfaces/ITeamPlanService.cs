using TheDugout.DTOs.Player;
using TheDugout.Models.Game;
using TheDugout.Models.Players;
using TheDugout.Models.Teams;

namespace TheDugout.Services.Team.Interfaces
{
    public interface ITeamPlanService
    {
        Dictionary<string, int> GetDefaultRosterPlan();
        Task<TeamTactic?> GetTeamTacticAsync(int teamId, int gameSaveId);
        Task<TeamTactic> SetTeamTacticAsync(
    int teamId,
    int tacticId,
    string? customName,
    Dictionary<string, string?> lineup,
    Dictionary<string, string?>? substitutes = null);
        Task InitializeDefaultTacticsAsync(GameSave gameSave);
        Task<TeamTactic> AutoPickTacticAsync(int teamId, int gameSaveId);

        Task<List<Models.Players.Player>> GetStartingLineupAsync(Models.Teams.Team team);
    }


}
