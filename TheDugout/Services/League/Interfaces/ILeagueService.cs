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
        Task GenerateTeamsForLeaguesAsync(GameSave gameSave, List<League> leagues);
        Task ProcessPromotionsAndRelegationsAsync(GameSave gameSave, Season previousSeason, List<League> newSeasonLeagues);
        Task<bool> IsLeagueFinishedAsync(int leagueId);
    }
}
