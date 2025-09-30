namespace TheDugout.Services.League
{
    public interface ILeagueFixturesService
    {
        Task GenerateLeagueFixturesAsync(int gameSaveId, int seasonId, DateTime startDate);
    }
}
