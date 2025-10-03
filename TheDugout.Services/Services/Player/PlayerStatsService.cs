using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Player
{
    public class PlayerStatsService : IPlayerStatsService
    {
        public List<PlayerMatchStats> InitializeMatchStats(Models.Matches.Match match)
        {
            var stats = new List<PlayerMatchStats>();

            if (match.Fixture.HomeTeam?.Players != null)
            {
                foreach (var player in match.Fixture.HomeTeam.Players.Where(p => p.IsActive))
                {
                    stats.Add(new PlayerMatchStats
                    {
                        PlayerId = player.Id,
                        Player = player,
                        MatchId = match.Id,
                        Match = match,
                        Goals = 0,
                    });
                }
            }

            if (match.Fixture.AwayTeam?.Players != null)
            {
                foreach (var player in match.Fixture.AwayTeam.Players.Where(p => p.IsActive))
                {
                    stats.Add(new PlayerMatchStats
                    {
                        PlayerId = player.Id,
                        Player = player,
                        MatchId = match.Id,
                        Match = match,
                        Goals = 0
                    });
                }
            }

            return stats;
        }

        public void UpdateStats(MatchEvent matchEvent, PlayerMatchStats stats)
        {
            switch (matchEvent.EventType.Code)
            {
                case "SHT":
                    if (matchEvent.Outcome.Name == "Goal")
                    {
                        stats.Goals++;
                    }
                    break;
                case "PAS":
                    // примерна логика...
                    break;
                case "TAC":
                    // примерна логика...
                    break;
                case "DRI":
                    // примерна логика...
                    break;
            }
        }

        public List<PlayerMatchStats> EnsureMatchStats(Models.Matches.Match match)
        {
            if (match.PlayerStats == null || !match.PlayerStats.Any())
            {
                var stats = InitializeMatchStats(match);
                match.PlayerStats = stats;
                return stats;
            }

            return match.PlayerStats.ToList();
        }
    }
}
