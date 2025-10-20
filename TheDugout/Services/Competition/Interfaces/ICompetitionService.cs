namespace TheDugout.Services.Competition.Interfaces
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Fixtures;
    public interface ICompetitionService
    {
        Task<bool> AreAllCompetitionsFinishedAsync(int seasonId);
        Task<List<CompetitionSeasonResult>> GenerateSeasonResultAsync(int seasonId);
        string GetCompetitionName(Fixture fixture);
        string GetCompetitionNameById(int competitionId);
    }
}
