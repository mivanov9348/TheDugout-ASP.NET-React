using TheDugout.Models;

namespace TheDugout.Services.Team
{
    public interface ITeamGenerationService
    {
        List<Models.Team> GenerateTeams(GameSave gameSave, Models.League league, IEnumerable<TeamTemplate> templates);

    }
}
