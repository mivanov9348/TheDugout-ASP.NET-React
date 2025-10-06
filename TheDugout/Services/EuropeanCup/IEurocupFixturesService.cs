namespace TheDugout.Services.EuropeanCup
{
    public interface IEurocupFixturesService
    {
        Task GenerateEuropeanLeaguePhaseFixturesAsync(int europeanCupId, int seasonId, CancellationToken ct = default);
        Task<List<Models.Fixtures.Fixture>> GetAllFixturesForCupAsync(int europeanCupId);
        Task<IEnumerable<object>> GetGroupFixturesAsync(int europeanCupId);


    }
}
