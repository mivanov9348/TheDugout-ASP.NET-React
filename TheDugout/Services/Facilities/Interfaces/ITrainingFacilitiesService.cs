namespace TheDugout.Services.Facilities
{
    using TheDugout.Models.Facilities;

    public interface ITrainingFacilitiesService
    {
        Task<TrainingFacility> AddTrainingFacilityAsync(int teamId);
        Task<bool> UpgradeTrainingFacilityAsync(int teamId);
        long? GetNextUpgradeCost(int currentLevel);
    }
}
