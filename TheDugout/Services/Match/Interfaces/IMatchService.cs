namespace TheDugout.Services.Match.Interfaces
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    public interface IMatchService
    {
        Task<Match> CreateMatchFromFixtureAsync(Models.Fixtures.Fixture fixture, GameSave gameSave);
        Task<object?> GetMatchViewAsync(int fixtureId);
        Task<object?> GetMatchViewByIdAsync(int matchId);
        Task CompleteMatchAndSaveResultAsync(Match match, int homeGoals, int awayGoals);
        Task<int> GenerateAttendanceAsync(Match match);
        Task<Match> GetOrCreateMatchAsync(Models.Fixtures.Fixture fixture, GameSave gameSave);
        Task SaveMatchProgressAsync(Match match);
    }
}
