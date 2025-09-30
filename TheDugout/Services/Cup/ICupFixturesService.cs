namespace TheDugout.Services.Cup
{
    public interface ICupFixturesService
    {
        Task GenerateAllCupFixturesAsync(
    int seasonId,
    int gameSaveId,
    List<Models.Cups.Cup> cups);

        Task GenerateNextRoundAsync(int cupId, int gameSaveId, int seasonId);

    }
}
