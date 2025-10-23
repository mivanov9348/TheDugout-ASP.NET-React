using TheDugout.Models.Game;
using TheDugout.Models.Players;
using TheDugout.Models.Staff;

namespace TheDugout.Services.Staff.Interfaces
{
    public interface IAgencyService
    {
        Task InitializeAgenciesForGameSaveAsync(GameSave save, CancellationToken ct = default);

    }
}
