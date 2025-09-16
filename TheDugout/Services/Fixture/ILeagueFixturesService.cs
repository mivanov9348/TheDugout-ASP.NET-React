namespace TheDugout.Services.Fixture
{
    public interface ILeagueFixturesService
    {
        Task GenerateLeagueFixturesAsync(int gameSaveId, int seasonId, DateTime startDate);
    }
}
