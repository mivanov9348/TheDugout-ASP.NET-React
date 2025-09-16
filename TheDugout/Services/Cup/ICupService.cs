using TheDugout.Models.Game;

namespace TheDugout.Services.Cup
{
    public interface ICupService
    {
        Task InitializeCupsForGameSaveAsync(GameSave gameSave, int seasonId);

    }

}
