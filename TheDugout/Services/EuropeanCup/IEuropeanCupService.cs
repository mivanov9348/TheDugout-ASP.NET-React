namespace TheDugout.Services.EuropeanCup
{
    public interface IEuropeanCupService
    {        
            Task<Models.Competitions.EuropeanCup> InitializeTournamentAsync(
                int templateId,
                int gameSaveId,
                int seasonId,
                CancellationToken ct = default);
            
        
        Task UpdateStandingsForPhaseAsync(int europeanCupPhaseId, CancellationToken ct = default);
        
    }
}
