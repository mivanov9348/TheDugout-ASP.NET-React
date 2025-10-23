namespace TheDugout.Services.Game.Interfaces
{
    using TheDugout.Models.Game;
    public interface IGameSaveService
    {
        Task<List<object>> GetUserSavesAsync(int userId);
        Task<GameSave?> GetGameSaveAsync(int userId, int saveId);
        Task<bool> DeleteGameSaveAsync(int saveId);
        Task<GameSave> StartNewGameAsync(int userId, CancellationToken ct = default);
    }
}
