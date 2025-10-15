namespace TheDugout.Services.Competition.Interfaces
{
    using TheDugout.Models.Competitions;
    public interface ICompetitionService
    {
        Task<bool> AreAllCompetitionsFinishedAsync(int seasonId);
        Task<List<CompetitionSeasonResult>> GenerateSeasonResultAsync(int seasonId);
    }
}
