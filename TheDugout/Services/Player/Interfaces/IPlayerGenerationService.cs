namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Staff;

    public interface IPlayerGenerationService
    {

        List<Player> GenerateTeamPlayers(GameSave save, Models.Teams.Team team);
        public Player? GenerateFreeAgent(GameSave save, Agency? agency = null);
        Task GeneratePlayersForAgenciesAsync(GameSave save, List<Agency> agencies, CancellationToken ct = default);
        string GetRandomAvatarFileName();

    }
}
