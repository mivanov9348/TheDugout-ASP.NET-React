namespace TheDugout.Services.EuropeanCup
{
    public interface IEurocupFixturesService
    {
        Task GenerateEuropeanLeaguePhaseFixturesAsync(int europeanCupId, int seasonId, CancellationToken ct = default);
    }
}
