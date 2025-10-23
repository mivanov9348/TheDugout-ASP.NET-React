namespace TheDugout.Services.EuropeanCup.Interfaces
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Fixtures;
    public interface IEuropeanCupService
    {        
            Task<EuropeanCup> InitializeTournamentAsync(
                int templateId,
                int gameSaveId,
                int seasonId,
                int? previousSeasonId,
                CancellationToken ct = default);           
        
        Task UpdateStandingsForPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default);
        Task<bool> IsEuropeanCupFinishedAsync(int euroCupId);
        Task HandleFinalMatchCompletionAsync(int europeanCupId, Fixture finalMatch, CancellationToken ct = default);

    }
}
