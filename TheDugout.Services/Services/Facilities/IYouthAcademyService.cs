namespace TheDugout.Services.Facilities
{
    public interface IYouthAcademyService
    {
        Task AddYouthAcademyAsync(int teamId);
        Task<bool> UpgradeYouthAcademyAsync(int teamId);
    }
}
