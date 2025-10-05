namespace TheDugout.Services.EuropeanCup
{
    public interface IEuropeanCupStandingService
    {
        Task UpdateEuropeanCupStandingsAfterMatchAsync(int fixtureId, CancellationToken ct = default);
        bool AreAllGroupMatchesPlayed(Models.Competitions.EuropeanCup cup);
        
    }
}
