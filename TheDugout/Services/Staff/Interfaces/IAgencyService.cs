namespace TheDugout.Services.Staff.Interfaces
{
    using TheDugout.Models.Game;

    public interface IAgencyService
    {
        Task InitializeAgenciesForGameSaveAsync(GameSave save, CancellationToken ct = default);
        Task DistributeSolidarityPaymentsAsync(GameSave save, decimal percentage = 20);

    }
}
