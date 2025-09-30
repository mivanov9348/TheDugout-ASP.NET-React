namespace TheDugout.Services.Standings
{
    public interface IEuropeanCupStandingService
    {
        Task UpdateEuropeanCupStandingsAfterMatchAsync(int fixtureId, CancellationToken ct = default);
    }
}
