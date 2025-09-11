using TheDugout.Models;

namespace TheDugout.Services.League
{
    public interface ILeagueService
    {
        Task<List<Models.League>> GenerateLeaguesAsync(GameSave gameSave, Models.Season season);
        Task InitializeStandingsAsync(GameSave gameSave, Models.Season season);
    }

}
