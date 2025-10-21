namespace TheDugout.Services.Cup.Interfaces
{
    using TheDugout.Models.Competitions;
    public interface ICupResultService
    {
        Task<List<CompetitionSeasonResult>> GenerateCupResultsAsync(int seasonId);
    }
}
