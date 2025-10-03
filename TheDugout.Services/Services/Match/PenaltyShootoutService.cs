using TheDugout.Models.Matches;
using TheDugout.Services.Team;
using TheDugout.Services.Player;

namespace TheDugout.Services.Match
{
    public class PenaltyShootoutService : IPenaltyShootoutService
    {
        private readonly IMatchEventService _matchEventService;
        private readonly IPlayerStatsService _playerStatsService;
        private readonly ITeamPlanService _teamPlanService;

        public PenaltyShootoutService(
            IMatchEventService matchEventService,
            IPlayerStatsService playerStatsService,
            ITeamPlanService teamPlanService)
        {
            _matchEventService = matchEventService;
            _playerStatsService = playerStatsService;
            _teamPlanService = teamPlanService;
        }

        public async Task<Models.Matches.Match> RunPenaltyShootoutAsync(Models.Matches.Match match)
        {
            // Материализираме lineup-ите като списъци още тук
            var homeLineup = (await _teamPlanService.GetStartingLineupAsync(match.Fixture.HomeTeam)).ToList();
            var awayLineup = (await _teamPlanService.GetStartingLineupAsync(match.Fixture.AwayTeam)).ToList();

            var homeQueue = new Queue<Models.Players.Player>(homeLineup);
            var awayQueue = new Queue<Models.Players.Player>(awayLineup);

            int homeScore = 0, awayScore = 0;

            // 1. Първите 5 дузпи
            for (int round = 1; round <= 5; round++)
            {
                homeScore += await TakePenaltyKick(match, match.Fixture.HomeTeamId, homeQueue, round);
                awayScore += await TakePenaltyKick(match, match.Fixture.AwayTeamId, awayQueue, round);

                if (IsDecided(homeScore, awayScore, round)) break;
            }

            // 2. Внезапна смърт
            int suddenRound = 6;
            while (homeScore == awayScore)
            {
                homeScore += await TakePenaltyKick(match, match.Fixture.HomeTeamId, homeQueue, suddenRound);
                awayScore += await TakePenaltyKick(match, match.Fixture.AwayTeamId, awayQueue, suddenRound);

                suddenRound++;

                if (suddenRound > 11) break; // safeguard
            }

            match.Fixture.WinnerTeamId = homeScore > awayScore
                ? match.Fixture.HomeTeamId
                : match.Fixture.AwayTeamId;

            return match;
        }

        private static bool IsDecided(int homeScore, int awayScore, int round)
        {
            return homeScore > awayScore + (5 - round) ||
                   awayScore > homeScore + (5 - round);
        }

        private async Task<int> TakePenaltyKick(Models.Matches.Match match, int teamId, Queue<Models.Players.Player> queue, int order)
        {
            var team = teamId == match.Fixture.HomeTeamId ? match.Fixture.HomeTeam : match.Fixture.AwayTeam;

            if (!queue.Any())
            {
                // Тук също материализираме списък
                var lineup = (await _teamPlanService.GetStartingLineupAsync(team)).ToList();
                foreach (var p in lineup) queue.Enqueue(p);
            }

            var player = queue.Dequeue();

            var eventType = _matchEventService.GetEventByCode("PEN");
            var outcome = _matchEventService.GetEventOutcome(player, eventType);
            bool scored = outcome.Name == "Goal";

            var penalty = new Penalty
            {
                MatchId = match.Id,
                TeamId = teamId,
                PlayerId = player.Id,
                Order = order,
                IsScored = scored
            };
            match.Penalties.Add(penalty);

            var commentary = _matchEventService.GetRandomCommentary(outcome, player);
            var matchEvent = _matchEventService.CreateMatchEvent(
                match.Id,
                120, // по-реалистично
                team,
                player,
                eventType,
                outcome,
                commentary
            );
            match.Events.Add(matchEvent);

            // Update stats
            var playerStats = match.PlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
            if (playerStats != null)
                _playerStatsService.UpdateStats(matchEvent, playerStats);

            return scored ? 1 : 0;
        }
    }
}
