namespace TheDugout.Services.Fixture
{
    public interface ICupFixturesService
    {
        Task GenerateAllCupFixturesAsync(
    int seasonId,
    int gameSaveId,
    List<Models.Competitions.Cup> cups);

    }
}
