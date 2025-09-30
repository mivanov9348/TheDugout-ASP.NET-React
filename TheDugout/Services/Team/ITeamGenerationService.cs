using System.Collections.Generic;
using System.Threading.Tasks;
using TheDugout.Models.Teams;
using TheDugout.Models.Game;

namespace TheDugout.Services.Team
{
    public interface ITeamGenerationService
    {
        Task<List<Models.Teams.Team>> GenerateTeamsAsync(
                GameSave gameSave,
                Models.Leagues.League league,
                IEnumerable<TeamTemplate> templates);
        Task<List<Models.Teams.Team>> GenerateIndependentTeamsAsync(GameSave gameSave);
    }
}
