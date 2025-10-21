namespace TheDugout.Services.League.Interfaces
{
    public interface ILeagueStandingsService
    {
        Task UpdateStandingsAfterMatchAsync(Models.Fixtures.Fixture fixture);
    }
}
