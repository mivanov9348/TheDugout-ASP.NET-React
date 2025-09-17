namespace TheDugout.Services.Season
{
    public interface ICupScheduleService
    {
        void AssignCupFixtures(List<Models.Matches.Fixture> fixtures, Models.Seasons.Season season);
    }
}
