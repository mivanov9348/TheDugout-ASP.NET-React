using TheDugout.Data.DtoNewGame;
using TheDugout.Models;

namespace TheDugout.Services.Game
{
    public interface IGameSaveService
    {
        Task<List<object>> GetUserSavesAsync(int userId);
        Task<GameSave?> GetGameSaveAsync(int userId, int saveId);
        Task<bool> DeleteGameSaveAsync(int userId, int saveId);
        Task<GameSave> StartNewGameAsync(int userId);
    }
}
