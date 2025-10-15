namespace TheDugout.Services.Season.Interfaces
{
    public interface ICupScheduleService
    {
        void AssignCupFixtures(List<Models.Fixtures.Fixture> fixtures, Models.Seasons.Season season);
    }
}
