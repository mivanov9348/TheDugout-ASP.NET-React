namespace TheDugout.Services.League.Interfaces
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    public interface ILeagueService
    {
        Task<List<Models.Leagues.League>> GenerateLeaguesAsync(GameSave gameSave, Models.Seasons.Season season);
        Task InitializeStandingsAsync(GameSave gameSave, Models.Seasons.Season season);
        Task<bool> IsLeagueFinishedAsync(int leagueId);
        Task<List<CompetitionSeasonResult>> GenerateLeagueResultsAsync(int seasonId);
    }
}
