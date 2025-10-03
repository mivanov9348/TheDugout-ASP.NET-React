using TheDugout.Models.Game;
using TheDugout.Models.Players;
using TheDugout.Models.Staff;

namespace TheDugout.Services.Staff
{
    public interface IAgencyService
    {
        Task InitializeAgenciesForGameSaveAsync(GameSave save, CancellationToken ct = default);

    }
}
