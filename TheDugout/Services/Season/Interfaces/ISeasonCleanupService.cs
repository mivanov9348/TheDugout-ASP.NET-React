namespace TheDugout.Services.Season.Interfaces
{
    public interface ISeasonCleanupService
    {
        Task CleanupOldSeasonDataAsync(int seasonId);
    }
}
