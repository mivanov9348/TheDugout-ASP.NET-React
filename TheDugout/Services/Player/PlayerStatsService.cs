using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Player
{
    public class PlayerStatsService : IPlayerStatsService
    {
        public PlayerStatsService()
        {
        }

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
                case "SHT": // Shot
                    if (matchEvent.Outcome.Name == "Goal")
                    {
                        stats.Goals++;
                        //stats.ShotsOnTarget++;
                    }
                    else if (matchEvent.Outcome.Name == "Saved" || matchEvent.Outcome.Name == "Blocked")
                    {
                        //stats.ShotsOnTarget++;
                    }
                    else if (matchEvent.Outcome.Name == "Out")
                    {
                        //stats.ShotsOffTarget++;
                    }
                    break;

                case "PAS": // Pass
                    if (matchEvent.Outcome.Name == "Success")
                    {
                        //stats.PassesCompleted++;
                    }
                    else
                    {
                        //stats.PassesFailed++;
                    }
                    break;

                case "TAC": // Tackle
                    if (matchEvent.Outcome.Name == "Success")
                    {
                        //stats.TacklesWon++;
                    }
                    else
                    {
                        //stats.TacklesLost++;
                    }
                    break;

                case "DRI": // Dribble
                    if (matchEvent.Outcome.Name == "Success")
                    {
                        //stats.DribblesCompleted++;
                    }
                    else
                    {
                        //stats.DribblesFailed++;
                    }
                    break;
            }
        }




    }
}
