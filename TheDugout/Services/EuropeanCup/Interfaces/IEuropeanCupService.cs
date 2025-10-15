namespace TheDugout.Services.EuropeanCup.Interfaces
{
    using TheDugout.Models.Competitions;
    public interface IEuropeanCupService
    {        
            Task<Models.Competitions.EuropeanCup> InitializeTournamentAsync(
                int templateId,
                int gameSaveId,
                int seasonId,
                CancellationToken ct = default);           
        
        Task UpdateStandingsForPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default);
        Task<bool> IsEuropeanCupFinishedAsync(int euroCupId);
        Task<List<CompetitionSeasonResult>> GenerateEuropeanCupResultsAsync(int seasonId);

    }
}
