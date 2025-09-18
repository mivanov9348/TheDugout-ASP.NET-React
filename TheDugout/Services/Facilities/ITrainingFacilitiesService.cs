namespace TheDugout.Services.Facilities
{
    public interface ITrainingFacilitiesService
    {
        Task AddTrainingFacilityAsync(int teamId);
        Task<bool> UpgradeTrainingFacilityAsync(int teamId);
    }
}
