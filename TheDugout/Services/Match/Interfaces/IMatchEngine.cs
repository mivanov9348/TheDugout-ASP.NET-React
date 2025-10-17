namespace TheDugout.Services.Match.Interfaces
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Fixtures;

    public interface IMatchEngine
    {
        Task EndMatch(Match match);
        void PlayNextMinute(Match match);
        void ChangeTurn(Match match);
        bool IsMatchFinished(Match match);
        Task<Match> SimulateMatchAsync(Fixture fixture, GameSave gameSave);
    }

}
