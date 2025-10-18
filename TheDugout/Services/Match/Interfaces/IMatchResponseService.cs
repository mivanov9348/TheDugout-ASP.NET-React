namespace TheDugout.Services.Match.Interfaces
{
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;

    public interface IMatchResponseService
    {
        Task<object> GetFormattedMatchResponseAsync(Fixture fixture, GameSave gameSave);
        Task<List<object>> GetFormattedMatchesResponseAsync(List<Fixture> fixtures, GameSave gameSave);
        string GetCompetitionName(Fixture fixture);
    }
}