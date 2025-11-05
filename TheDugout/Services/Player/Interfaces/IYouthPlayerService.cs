namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.DTOs.Player;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    public interface IYouthPlayerService
    {
        Task GenerateAllYouthIntakesAsync(YouthAcademy academy, GameSave gameSave, CancellationToken ct = default);
        Task<List<PlayerDto>> GetYouthPlayersByTeamAsync(int teamId);
    }
}
