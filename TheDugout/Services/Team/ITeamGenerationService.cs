using TheDugout.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheDugout.Services.Team
{
    public interface ITeamGenerationService
    {
        List<Models.Team> GenerateTeams(GameSave gameSave, Models.League league, IEnumerable<TeamTemplate> templates);

        Task<List<Models.Team>> GenerateIndependentTeamsAsync(GameSave gameSave);
    }
}
