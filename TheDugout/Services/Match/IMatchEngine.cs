namespace TheDugout.Services.MatchEngine
{
    public interface IMatchEngine
    {
        void StartMatch(Models.Matches.Match match);
        void EndMatch(Models.Matches.Match match);
        void PlayNextMinute(Models.Matches.Match match);
        void ChangeTurn(Models.Matches.Match match);
        bool IsMatchFinished(Models.Matches.Match match);
        Task RunMatch(Models.Matches.Match match);
    }

}
