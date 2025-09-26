

namespace TheDugout.Services.MatchEngine
{
    public interface IMatchEngine
    {
        void StartMatch(Models.Matches.Match match);

        void NextMinute(Models.Matches.Match match);

        int GetCurrentTurn(Models.Matches.Match match);

        void EndMatch(Models.Matches.Match match);
    }
}
