namespace TheDugout.Services.Season.Interfaces
{
    public interface ISeasonEndService
    {
        Task<bool> ProcessSeasonEndAsync(int seasonId);
    }
}
