namespace TheDugout.Services.Season
{
    public interface IEurocupScheduleService
    {
        List<DateTime> AssignEuropeanFixtures(Models.Seasons.Season season, int rounds);
    }
}
