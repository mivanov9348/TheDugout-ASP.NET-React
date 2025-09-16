namespace TheDugout.Services.Fixture
{
    public interface IEurocupFixturesService
    {
        Task GenerateEuropeanLeaguePhaseFixturesAsync(int europeanCupId, int seasonId, CancellationToken ct = default);

        Task GenerateEuropeanKnockoutFixturesAsync(int europeanCupId, int knockoutPhaseTemplateId, CancellationToken ct = default);
    }
}
