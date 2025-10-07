using TheDugout.Models.Game;
using TheDugout.Models.Matches;

namespace TheDugout.Services.MatchEngine
{
    public interface IMatchEngine
    {
        Task StartMatch(Models.Matches.Match match);
        Task EndMatch(Models.Matches.Match match);
        void PlayNextMinute(Models.Matches.Match match);
        void ChangeTurn(Models.Matches.Match match);
        bool IsMatchFinished(Models.Matches.Match match);
        Task<MatchEvent?> PlayStep(Models.Matches.Match match);
        Task<Models.Matches.Match> SimulateMatchAsync(Models.Fixtures.Fixture fixture, GameSave gameSave);
        Task RunMatch(Models.Matches.Match match);
    }

}
