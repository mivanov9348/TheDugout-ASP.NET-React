namespace TheDugout.Services.Game
{
    using TheDugout.Models.Game;
    public interface IGameSaveService
    {
        Task<List<object>> GetUserSavesAsync(int userId);
        Task<GameSave?> GetGameSaveAsync(int userId, int saveId);
        Task<bool> DeleteGameSaveAsync(int saveId);
        Task<GameSave> StartNewGameAsync(int userId, Func<string, Task>? progress = null, CancellationToken ct = default);
    }
}
