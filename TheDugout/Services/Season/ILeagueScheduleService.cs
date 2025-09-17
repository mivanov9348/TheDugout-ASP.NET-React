namespace TheDugout.Services.Season
{
    public interface ILeagueScheduleService
    {
        void AssignLeagueFixtures(List<Models.Matches.Fixture> fixtures, Models.Seasons.Season season);

    }
}
