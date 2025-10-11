using TheDugout.Models.Facilities;

namespace TheDugout.Services.Facilities
{
    public interface IStadiumService
    {
        Task<Stadium> AddStadiumAsync(int teamId);
        Task<bool> UpgradeStadiumAsync(int teamId);

    }
}
