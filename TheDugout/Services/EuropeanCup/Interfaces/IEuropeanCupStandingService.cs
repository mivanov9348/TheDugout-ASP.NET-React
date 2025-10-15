namespace TheDugout.Services.EuropeanCup.Interfaces
{
    public interface IEuropeanCupStandingService
    {
        Task UpdateEuropeanCupStandingsAfterMatchAsync(int fixtureId, CancellationToken ct = default);
        bool AreAllGroupMatchesPlayed(Models.Competitions.EuropeanCup cup);
        Task<IEnumerable<object>> GetSortedStandingsAsync(int cupId);
    }
}
