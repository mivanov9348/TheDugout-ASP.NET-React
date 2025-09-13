using TheDugout.Models.Game;
using TheDugout.Models.Players;

namespace TheDugout.Services.Players
{
    public interface IPlayerGenerationService
    {
        
        List<Player> GenerateTeamPlayers(GameSave save, Models.Teams.Team team);
        List<Player> GenerateFreeAgents(GameSave save, int count = 100);

    }
}
