namespace TheDugout.Services.Fixture
{
    public interface ICupFixturesService
    {
        Task GenerateCupFixturesAsync(Models.Competitions.Cup cup, int seasonId, int gameSaveId, bool shareCupStartDate = true, bool markSeasonEvent = false);

    }
}
