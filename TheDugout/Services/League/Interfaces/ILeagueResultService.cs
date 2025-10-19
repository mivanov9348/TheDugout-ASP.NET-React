namespace TheDugout.Services.League.Interfaces
{
    using TheDugout.Models.Competitions;
    public interface ILeagueResultService
    {
        Task<List<CompetitionSeasonResult>> GenerateLeagueResultsAsync(int seasonId);
    }
}
