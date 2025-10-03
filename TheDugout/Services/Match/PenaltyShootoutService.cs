using TheDugout.Models.Matches;
using TheDugout.Models.Players;
using TheDugout.Services.Player;

namespace TheDugout.Services.Match
{
    public class PenaltyShootoutService : IPenaltyShootoutService
    {
        private readonly Random _random = new Random();
        private readonly IMatchEventService _matchEventService;
        private readonly IPlayerStatsService _playerStatsService;

        public PenaltyShootoutService(
            IMatchEventService matchEventService,
            IPlayerStatsService playerStatsService)
        {
            _matchEventService = matchEventService;
            _playerStatsService = playerStatsService;
        }

        public async Task<Models.Matches.Match> RunPenaltyShootoutAsync(Models.Matches.Match match)
        {
            // 1. Първите 5 дузпи
            await RunInitialSeriesAsync(match);

            // 2. Проверка за равенство
            var homeScore = match.Penalties.Count(p => p.TeamId == match.Fixture.HomeTeamId && p.IsScored);
            var awayScore = match.Penalties.Count(p => p.TeamId == match.Fixture.AwayTeamId && p.IsScored);

            // 3. Ако още е равен → sudden death
            if (homeScore == awayScore)
            {
                await RunSuddenDeathAsync(match, homeScore, awayScore);
            }

            return match;
        }

        private async Task RunInitialSeriesAsync(Models.Matches.Match match)
        {
            var homeTeamId = match.Fixture.HomeTeamId;
            var awayTeamId = match.Fixture.AwayTeamId;

            for (int i = 1; i <= 5; i++)
            {
                await TakePenaltyAsync(match, homeTeamId, i);
                await TakePenaltyAsync(match, awayTeamId, i);
            }
        }

        private async Task RunSuddenDeathAsync(Models.Matches.Match match, int homeScore, int awayScore)
        {
            var homeTeamId = match.Fixture.HomeTeamId;
            var awayTeamId = match.Fixture.AwayTeamId;

            int order = 6; // започваме след 5-тата дузпа

            while (homeScore == awayScore)
            {
                var homePenalty = await TakePenaltyAsync(match, homeTeamId, order);
                var awayPenalty = await TakePenaltyAsync(match, awayTeamId, order);

                if (homePenalty.IsScored) homeScore++;
                if (awayPenalty.IsScored) awayScore++;

                order++;
            }
        }

        private async Task<Penalty> TakePenaltyAsync(Models.Matches.Match match, int teamId, int order)
        {
            var team = teamId == match.Fixture.HomeTeamId ? match.Fixture.HomeTeam : match.Fixture.AwayTeam;
            var lineup = team?.Players.Where(p => p.Position.Code != "GK").ToList() ?? new List<Models.Players.Player>();

            if (!lineup.Any())
                throw new InvalidOperationException($"No penalty takers found for team {teamId}");

            var player = lineup[_random.Next(lineup.Count)];

            // Random шанс за гол (примерно 75%)
            bool isScored = _random.NextDouble() < 0.75;

            var penalty = new Penalty
            {
                MatchId = match.Id,
                TeamId = teamId,
                PlayerId = player.Id,
                Order = order,
                IsScored = isScored
            };

            match.Penalties.Add(penalty);

            //// (По желание) създаване на MatchEvent за дузпата
            //var eventType = _matchEventService.GetRandomEvent(); 

            //var outcome = isScored
            //    ? _matchEventService.GetOutcomeByName("Goal")
            //    : _matchEventService.GetOutcomeByName("Miss");

            //var commentary = _matchEventService.GetRandomCommentary(outcome, player);
            //var matchEvent = _matchEventService.CreateMatchEvent(
            //    match.Id,
            //    90, // винаги след мача
            //    team!,
            //    player,
            //    eventType,
            //    outcome,
            //    commentary
            //);
            //match.Events.Add(matchEvent);

            // Update stats
            //var playerStats = match.PlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
            //if (playerStats != null)
            //{
            //    _playerStatsService.UpdateStats(matchEvent, playerStats);
            //}

            return penalty;
        }
    }
}
