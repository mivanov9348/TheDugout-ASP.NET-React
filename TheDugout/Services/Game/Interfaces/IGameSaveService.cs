namespace TheDugout.Services.Game.Interfaces
{
    using System.Runtime.CompilerServices;
    using TheDugout.Models.Game;
    public interface IGameSaveService
    {
        Task<List<object>> GetUserSavesAsync(int userId);
        Task<GameSave?> GetGameSaveAsync(int userId, int saveId);
        Task<bool> DeleteGameSaveAsync(int saveId);
        IAsyncEnumerable<string> StartNewGameStreamAsync(int? userId, CancellationToken ct = default);
        //Task<GameSave> StartNewGameAsync(int userId, CancellationToken ct = default);
    }
}
