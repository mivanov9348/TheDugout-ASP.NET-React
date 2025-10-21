namespace TheDugout.Services.EuropeanCup.Interfaces
{
    using TheDugout.Models.Competitions;

    public interface IEuropeanCupResultService
    {
        Task<List<CompetitionSeasonResult>> GenerateEuropeanCupResultsAsync(int seasonId);
    }
}
