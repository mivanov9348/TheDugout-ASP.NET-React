using TheDugout.Models;

namespace TheDugout.Services.Players
{
    public interface IPlayerGenerationService
    {
        
        List<Models.Player> GenerateTeamPlayers(GameSave save, Models.Team team);
        List<Player> GenerateFreeAgents(GameSave save, int count = 100);

    }
}
