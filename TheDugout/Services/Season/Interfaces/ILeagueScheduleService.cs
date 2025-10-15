namespace TheDugout.Services.Season.Interfaces
{
    public interface ILeagueScheduleService
    {
        void AssignLeagueFixtures(List<Models.Fixtures.Fixture> fixtures, Models.Seasons.Season season);

    }
}
