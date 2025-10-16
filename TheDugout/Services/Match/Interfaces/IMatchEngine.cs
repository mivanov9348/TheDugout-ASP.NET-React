namespace TheDugout.Services.Match.Interfaces
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;

    public interface IMatchEngine
    {
        Task StartMatch(Match match);
        Task EndMatch(Match match);
        void PlayNextMinute(Match match);
        void ChangeTurn(Match match);
        bool IsMatchFinished(Match match);
        Task<MatchEvent?> PlayStep(Match match);
        Task<Match> SimulateMatchAsync(Models.Fixtures.Fixture fixture, GameSave gameSave);
        Task RunMatch(Match match);
    }

}
