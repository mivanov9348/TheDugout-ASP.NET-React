namespace TheDugout.Services.EuropeanCup
{
    public interface IEuropeanCupService
    {        
            Task<Models.EuropeanCup> InitializeTournamentAsync(
                int templateId,
                int gameSaveId,
                int seasonId,
                CancellationToken ct = default);

            Task GenerateLeaguePhaseFixturesAsync(
                int europeanCupId,
                int seasonId,
                CancellationToken ct = default);        

        Task CreateStandingsIfNotExistsAsync(int europeanCupId, CancellationToken ct = default);
        Task RecordFixtureResultAsync(int fixtureId, int homeGoals, int awayGoals, CancellationToken ct = default);
        Task UpdateStandingsForPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default);
        Task ProgressToNextPhaseIfReadyAsync(int europeanCupId, CancellationToken ct = default);
        Task GenerateKnockoutFixturesAsync(int europeanCupId, int knockoutPhaseTemplateId, CancellationToken ct = default);
        Task ResolveKnockoutPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default);
        Task FinalizeTournamentAsync(int europeanCupId, CancellationToken ct = default);
        Task<Models.EuropeanCup?> GetByIdAsync(int europeanCupId, CancellationToken ct = default);
    }
}
