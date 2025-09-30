using TheDugout.Models.Game;

namespace TheDugout.Services.League
{
    public interface ILeagueService
    {
        Task<List<Models.Leagues.League>> GenerateLeaguesAsync(GameSave gameSave, Models.Seasons.Season season);
        Task InitializeStandingsAsync(GameSave gameSave, Models.Seasons.Season season);
    }

}
