namespace TheDugout.Services.Player.Interfaces
{
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    public interface IPlayerStatsService
    {
        Task<List<PlayerMatchStats>> InitializeMatchStatsAsync(Match match);
        void UpdateStats(MatchEvent matchEvent, PlayerMatchStats stats);
        Task<List<PlayerMatchStats>> EnsureMatchStatsAsync(Match match);
        Task UpdateCompetitionStatsAfterMatchAsync(Match match);
        Task UpdateSeasonStatsAfterMatchAsync(Match match);
        double CalculateMatchRating(PlayerMatchStats stat);
        double CalculateSeasonRating(IEnumerable<PlayerMatchStats> matchStats);
        Task<List<(int CompetitionId, int PlayerId, int Goals)>> GetTopScorersByCompetitionAsync(int seasonId);

    }
}
