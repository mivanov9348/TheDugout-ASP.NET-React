using TheDugout.Models.Game;
using TheDugout.Models.Players;
using TheDugout.Models.Staff;

namespace TheDugout.Services.Players
{
    public interface IPlayerGenerationService
    {

        List<Player> GenerateTeamPlayers(GameSave save, Models.Teams.Team team);
        public Player? GenerateFreeAgent(GameSave save, Agency? agency = null);

    }
}
