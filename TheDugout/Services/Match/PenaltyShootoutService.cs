using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Matches;
using TheDugout.Services.Player;
using TheDugout.Services.Team;

namespace TheDugout.Services.Match
{
    public class PenaltyShootoutService : IPenaltyShootoutService
    {
        private readonly DugoutDbContext _context;
        private readonly IMatchEventService _matchEventService;
        private readonly IPlayerStatsService _playerStatsService;
        private readonly ITeamPlanService _teamPlanService;

        public PenaltyShootoutService(
            DugoutDbContext context,
            IMatchEventService matchEventService,
            IPlayerStatsService playerStatsService,
            ITeamPlanService teamPlanService)
        {
            _context = context;
            _matchEventService = matchEventService;
            _playerStatsService = playerStatsService;
            _teamPlanService = teamPlanService;
        }

        public async Task<Models.Matches.Match> RunPenaltyShootoutAsync(Models.Matches.Match match)
        {
            var homeLineup = (await _teamPlanService.GetStartingLineupAsync(match.Fixture.HomeTeam)).ToList();
            var awayLineup = (await _teamPlanService.GetStartingLineupAsync(match.Fixture.AwayTeam)).ToList();

            var homeQueue = new Queue<Models.Players.Player>(homeLineup);
            var awayQueue = new Queue<Models.Players.Player>(awayLineup);

            var penaltyEventType = _matchEventService.GetEventByCode("PEN");

            int homeScore = 0, awayScore = 0;

            // първи 5
            for (int round = 1; round <= 5; round++)
            {
                homeScore += await TakePenaltyKick(match, match.Fixture.HomeTeam.Id, homeQueue, round, penaltyEventType);
                awayScore += await TakePenaltyKick(match, match.Fixture.AwayTeam.Id, awayQueue, round, penaltyEventType);

                if (IsDecided(homeScore, awayScore, round))
                    break;
            }

            // sudden death
            int suddenRound = 6;
            while (homeScore == awayScore)
            {
                homeScore += await TakePenaltyKick(match, match.Fixture.HomeTeam.Id, homeQueue, suddenRound, penaltyEventType);
                awayScore += await TakePenaltyKick(match, match.Fixture.AwayTeam.Id, awayQueue, suddenRound, penaltyEventType);

                suddenRound++;

                if (!homeQueue.Any())
                    homeQueue = new Queue<Models.Players.Player>(
                        (await _teamPlanService.GetStartingLineupAsync(match.Fixture.HomeTeam)).ToList());

                if (!awayQueue.Any())
                    awayQueue = new Queue<Models.Players.Player>(
                        (await _teamPlanService.GetStartingLineupAsync(match.Fixture.AwayTeam)).ToList());
            }

            // запазваме само победител, НЕ пипаме HomeTeamGoals / AwayTeamGoals
            match.Fixture.WinnerTeamId = homeScore > awayScore
                ? match.Fixture.HomeTeamId
                : match.Fixture.AwayTeamId;            

            _context.Matches.Update(match);
            _context.Fixtures.Update(match.Fixture);
            await _context.SaveChangesAsync();

            return match;
        }

        private static bool IsDecided(int homeScore, int awayScore, int round)
        {
            return homeScore > awayScore + (5 - round) ||
                   awayScore > homeScore + (5 - round);
        }

        private async Task<int> TakePenaltyKick(
            Models.Matches.Match match,
            int teamId,
            Queue<Models.Players.Player> queue,
            int order,
            EventType penaltyEventType)
        {
            var team = teamId == match.Fixture.HomeTeamId
                ? match.Fixture.HomeTeam
                : match.Fixture.AwayTeam;

            // Ако свършат хората — зареждаме lineup наново
            if (!queue.Any())
            {
                var lineup = (await _teamPlanService.GetStartingLineupAsync(team)).ToList();
                foreach (var p in lineup) queue.Enqueue(p);
            }

            var player = queue.Dequeue();

            // Изчисляваме изход
            var goalkeeper = teamId == match.Fixture.HomeTeamId
                ? match.Fixture.AwayTeam.Players.FirstOrDefault(p => p.Position.Code == "GK")
                : match.Fixture.HomeTeam.Players.FirstOrDefault(p => p.Position.Code == "GK");

            var outcome = _matchEventService.GetPenaltyOutcome(player, goalkeeper, penaltyEventType);
            
            bool scored = outcome.Name == "Goal";

            // Commentary
            var commentary = _matchEventService.GetRandomCommentary(outcome, player);

            // MatchEvent (фиктивна минута 120)
            var matchEvent = await _matchEventService.CreateMatchEvent(
                match.Id,
                120,
                team,
                player,
                penaltyEventType,
                outcome,
                commentary
            );

            match.Events.Add(matchEvent);

            // PlayerStats
            var playerStats = match.PlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
            if (playerStats != null)
                _playerStatsService.UpdateStats(matchEvent, playerStats);

            // Записваме дузпата
            var penalty = new Penalty
            {
                MatchId = match.Id,
                TeamId = teamId,
                PlayerId = player.Id,
                Order = order,
                IsScored = scored
            };
            match.Penalties.Add(penalty);
            await _context.Penalties.AddAsync(penalty);

            await _context.SaveChangesAsync();

            return scored ? 1 : 0;
        }
    }
}

