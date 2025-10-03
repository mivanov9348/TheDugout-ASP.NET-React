namespace TheDugout.Services.Season
{
    public interface ILeagueScheduleService
    {
        void AssignLeagueFixtures(List<Models.Fixtures.Fixture> fixtures, Models.Seasons.Season season);

    }
}
