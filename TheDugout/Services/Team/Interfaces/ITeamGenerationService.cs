namespace TheDugout.Services.Team.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Teams;

    public interface ITeamGenerationService
    {
        Task<List<Team>> GenerateTeamsAsync(
                GameSave gameSave,
                League league,
                IEnumerable<TeamTemplate> templates);
        Task EnsureTeamRostersAsync(int gameSaveId);
        Task<List<Team>> GenerateIndependentTeamsAsync(GameSave gameSave);

        Task UpdatePopularityAsync(Team team, TeamEventType eventType);
    }
}
