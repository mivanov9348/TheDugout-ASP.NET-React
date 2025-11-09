namespace TheDugout.Services.Standings.Interfaces
{
    public interface IStandingsDispatcherService
    {
        Task UpdateAfterMatchAsync(Models.Fixtures.Fixture fixture, CancellationToken ct = default);
    }
}
