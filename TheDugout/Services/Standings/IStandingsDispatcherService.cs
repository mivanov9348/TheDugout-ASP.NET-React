namespace TheDugout.Services.Standings
{
    public interface IStandingsDispatcherService
    {
        Task UpdateAfterMatchAsync(Models.Fixtures.Fixture fixture, CancellationToken ct = default);
    }
}
