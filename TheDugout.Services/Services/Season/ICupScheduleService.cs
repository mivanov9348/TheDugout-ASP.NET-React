namespace TheDugout.Services.Season
{
    public interface ICupScheduleService
    {
        void AssignCupFixtures(List<Models.Fixtures.Fixture> fixtures, Models.Seasons.Season season);
    }
}
