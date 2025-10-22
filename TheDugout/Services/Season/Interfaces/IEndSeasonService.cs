namespace TheDugout.Services.Season.Interfaces
{
    public interface IEndSeasonService
    {
        Task<bool> ProcessSeasonEndAsync(int seasonId);
        Task<bool> StartNewSeasonAsync(int seasonId);
    }
}
