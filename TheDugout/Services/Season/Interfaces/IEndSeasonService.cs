namespace TheDugout.Services.Season.Interfaces
{
    using TheDugout.Models.Seasons;

    public interface IEndSeasonService
    {
        Task<bool> ProcessSeasonEndAsync(int seasonId);
    }
}
