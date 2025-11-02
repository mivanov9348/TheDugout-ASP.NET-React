namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    public interface IYouthPlayerService
    {
        Task GenerateYouthIntakeAsync(YouthAcademy academy, GameSave save);
        Task<List<Player>> GetYouthPlayersByTeamAsync(int teamId);
    }
}
