namespace TheDugout.Services.Player
{
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    public interface IPlayerStatsService
    {
        Task<List<PlayerMatchStats>> InitializeMatchStatsAsync(Models.Matches.Match match);
        void UpdateStats(MatchEvent matchEvent, PlayerMatchStats stats);
        Task<List<PlayerMatchStats>> EnsureMatchStatsAsync(Models.Matches.Match match);
        Task UpdateSeasonStatsAfterMatchAsync(Models.Matches.Match match);

    }
}
