using TheDugout.Models.Game;
using TheDugout.Models.Players;
using TheDugout.Models.Staff;

namespace TheDugout.Services.Player.Interfaces
{
    public interface IPlayerGenerationService
    {

        List<Models.Players.Player> GenerateTeamPlayers(GameSave save, Models.Teams.Team team);
        public Models.Players.Player? GenerateFreeAgent(GameSave save, Agency? agency = null);
        string GetRandomAvatarFileName();

    }
}
