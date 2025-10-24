namespace TheDugout.Services.Facilities
{
    using TheDugout.Models.Facilities;
    public interface IYouthAcademyService
    {
        Task<YouthAcademy> AddYouthAcademyAsync(int teamId);
        Task<bool> UpgradeYouthAcademyAsync(int teamId);
        long? GetNextUpgradeCost(int currentLevel);
    }
}
