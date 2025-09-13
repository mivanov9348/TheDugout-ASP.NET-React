using System.Collections.Generic;
using System.Threading.Tasks;
using TheDugout.Models.Teams;
using TheDugout.Models.Game;

namespace TheDugout.Services.Team
{
    public interface ITeamGenerationService
    {
        List<Models.Teams.Team> GenerateTeams(GameSave gameSave, Models.Competitions.League league, IEnumerable<TeamTemplate> templates);

        Task<List<Models.Teams.Team>> GenerateIndependentTeamsAsync(GameSave gameSave);
    }
}
