namespace TheDugout.Services.Team.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TheDugout.Models.Teams;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;

    public interface ITeamGenerationService
    {
        Task<List<Team>> GenerateTeamsAsync(
                GameSave gameSave,
                League league,
                IEnumerable<TeamTemplate> templates);
        Task EnsureTeamRostersAsync(int gameSaveId);
        Task<List<Team>> GenerateIndependentTeamsAsync(GameSave gameSave);
    }
}
