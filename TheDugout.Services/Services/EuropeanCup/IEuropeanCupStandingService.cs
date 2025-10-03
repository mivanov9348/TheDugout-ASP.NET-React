namespace TheDugout.Services.EuropeanCup
{
    public interface IEuropeanCupStandingService
    {
        Task UpdateEuropeanCupStandingsAfterMatchAsync(int fixtureId, CancellationToken ct = default);
    }
}
