using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Player
{
    public interface IPlayerStatsService
    {
        List<PlayerMatchStats> InitializeMatchStats(Models.Matches.Match match);
        void UpdateStats(MatchEvent matchEvent, PlayerMatchStats stats);
        List<PlayerMatchStats> EnsureMatchStats(Models.Matches.Match match);

    }
}
