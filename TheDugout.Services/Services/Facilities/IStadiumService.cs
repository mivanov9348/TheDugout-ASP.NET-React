namespace TheDugout.Services.Facilities
{
    public interface IStadiumService
    {
        Task AddStadiumAsync(int teamId);
        Task<bool> UpgradeStadiumAsync(int teamId);

    }
}
