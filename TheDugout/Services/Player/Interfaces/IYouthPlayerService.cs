namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    public interface IYouthPlayerService
    {
        Task GenerateAllYouthIntakesAsync(YouthAcademy academy, GameSave gameSave, CancellationToken ct = default);
        Task<List<Player>> GetYouthPlayersByTeamAsync(int teamId);
    }
}
