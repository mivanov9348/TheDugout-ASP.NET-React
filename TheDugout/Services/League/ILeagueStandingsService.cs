namespace TheDugout.Services.League
{
    public interface ILeagueStandingsService
    {
        Task UpdateStandingsAfterMatchAsync(Models.Fixtures.Fixture fixture);

    }
}
