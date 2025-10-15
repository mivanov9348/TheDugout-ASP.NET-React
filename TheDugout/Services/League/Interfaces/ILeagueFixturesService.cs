namespace TheDugout.Services.League.Interfaces
{
    public interface ILeagueFixturesService
    {
        Task GenerateLeagueFixturesAsync(int gameSaveId, int seasonId, DateTime startDate);
    }
}
