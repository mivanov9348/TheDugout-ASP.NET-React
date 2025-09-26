using TheDugout.Models.Game;

namespace TheDugout.Services.Match
{
    public interface IMatchService
    {
        Task<TheDugout.Models.Matches.Match> CreateMatchFromFixtureAsync(Models.Matches.Fixture fixture, GameSave gameSave);
        Task<object?> GetMatchViewAsync(int fixtureId);
        Task<object?> GetMatchViewByIdAsync(int matchId);
        Task CompleteMatchAndSaveResultAsync(Models.Matches.Match match, int homeGoals, int awayGoals);



    }
}
