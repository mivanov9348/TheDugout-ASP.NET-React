namespace TheDugout.Services.Facilities
{
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Matches;
    public interface IStadiumService
    {
        Task<Stadium> AddStadiumAsync(int teamId);
        Task<bool> UpgradeStadiumAsync(int teamId);
        long? GetNextUpgradeCost(int currentLevel);
        Task<decimal> GenerateMatchRevenueAsync(Match match);
    }
}
