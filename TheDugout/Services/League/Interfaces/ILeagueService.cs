namespace TheDugout.Services.League.Interfaces
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Seasons;
    public interface ILeagueService
    {
        Task<List<League>> GenerateLeaguesAsync(GameSave gameSave, Season season);
        Task InitializeStandingsAsync(GameSave gameSave, Season season);
        Task<bool> IsLeagueFinishedAsync(int leagueId);
    }
}
