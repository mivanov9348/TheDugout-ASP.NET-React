namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.Models.Players;
    public interface IShortlistPlayerService
    {
        Task AddToShortlistAsync(int gameSaveId, int playerId, int? userId = null, int? teamId = null, string? note = null);

        Task RemoveFromShortlistAsync(int gameSaveId, int playerId, int? userId = null, int? teamId = null);

        Task<List<Player>> GetShortlistPlayersAsync(int gameSaveId, int? userId = null, int? teamId = null);
    }
}
