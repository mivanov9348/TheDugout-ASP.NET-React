namespace TheDugout.Services.Match.Interfaces
{
    using TheDugout.Models.Game;
    public interface IMatchService
    {
        Task<Models.Matches.Match> CreateMatchFromFixtureAsync(Models.Fixtures.Fixture fixture, GameSave gameSave);
        Task<object?> GetMatchViewAsync(int fixtureId);
        Task<object?> GetMatchViewByIdAsync(int matchId);
        Task CompleteMatchAndSaveResultAsync(Models.Matches.Match match, int homeGoals, int awayGoals);
        Task<Models.Matches.Match> GetOrCreateMatchAsync(Models.Fixtures.Fixture fixture, GameSave gameSave);
    }
}
