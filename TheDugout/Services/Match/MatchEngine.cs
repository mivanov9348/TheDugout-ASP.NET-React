namespace TheDugout.Services.MatchEngine
{
    using Azure;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Match.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Standings;
    using TheDugout.Services.Team.Interfaces;

    public class MatchEngine : IMatchEngine
    {
        private readonly Random _random = new Random();
        private readonly ITeamPlanService _teamPlanService;
        private readonly IMatchService _matchService;
        private readonly IMatchEventService _matchEventService;
        private readonly IPlayerStatsService _playerStatsService;
        private readonly ILeagueStandingsService _leagueStandingsService;
        private readonly IStandingsDispatcherService _standingsDispatcher;
        private readonly IPenaltyShootoutService _penaltyService;
        private readonly DugoutDbContext _context;

        public MatchEngine(
            ITeamPlanService teamPlanService,
            IMatchEventService matchEventService,
            IPlayerStatsService playerStatsService,
            ILeagueStandingsService leagueStandingsService,
            IStandingsDispatcherService standingsDispatcher,
        IMatchService matchService,
            IPenaltyShootoutService penaltyService,
            DugoutDbContext context)
        {
            _teamPlanService = teamPlanService;
            _matchEventService = matchEventService;
            _playerStatsService = playerStatsService;
            _leagueStandingsService = leagueStandingsService;
            _matchService = matchService;
            _standingsDispatcher = standingsDispatcher;
            _penaltyService = penaltyService;
            _context = context;
        }
        public async Task EndMatch(Match match)
        {
            match.Status = MatchStageEnum.Played;

            var fixture = match.Fixture;
            fixture.Status = MatchStageEnum.Played;

            int homeGoals = fixture.HomeTeamGoals ?? 0;
            int awayGoals = fixture.AwayTeamGoals ?? 0;

            if (homeGoals > awayGoals)
            {
                fixture.WinnerTeamId = fixture.HomeTeamId;
            }
            else if (awayGoals > homeGoals)
            {
                fixture.WinnerTeamId = fixture.AwayTeamId;
            }
            else
            {
                fixture.WinnerTeamId = null;
            }

            if (match.PlayerStats != null && match.PlayerStats.Any())
            {
                await _playerStatsService.UpdateStatsAfterMatchAsync(match);
            }
        }
        public void PlayNextMinute(Match match)
        {
            int increment = _random.Next(1, 21);
            match.CurrentMinute += increment;
        }
        public void ChangeTurn(Match match)
        {
            match.CurrentTurn = match.CurrentTurn == MatchTurn.Home
                ? MatchTurn.Away
                : MatchTurn.Home;
        }
        public bool IsMatchFinished(Match match)
        {
            return match.CurrentMinute >= 90;
        }      

        public async Task<Match> SimulateMatchAsync(Fixture fixture, GameSave gameSave)
        {
            if (fixture is null) throw new ArgumentNullException(nameof(fixture));
            if (gameSave is null) throw new ArgumentNullException(nameof(gameSave));

            var sw = Stopwatch.StartNew();

            var dbFixture = await _context.Fixtures
                .Include(f => f.HomeTeam).ThenInclude(t => t.Players)
                .Include(f => f.AwayTeam).ThenInclude(t => t.Players)
                .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(f => f.Id == fixture.Id);

            if (dbFixture is null)
                throw new InvalidOperationException($"Fixture with Id {fixture.Id} not found in DB.");

            var match = await _matchService.GetOrCreateMatchAsync(fixture, gameSave);

            match.PlayerStats = await _playerStatsService.EnsureMatchStatsAsync(match);

            int eventCount = 0;

            while (!IsMatchFinished(match))
            {
                PlayNextMinute(match);

                var currentTeamId = match.CurrentTurn == MatchTurn.Home
                    ? match.Fixture.HomeTeamId
                    : match.Fixture.AwayTeamId;

                var currentTeam = match.CurrentTurn == MatchTurn.Home
                    ? match.Fixture.HomeTeam
                    : match.Fixture.AwayTeam;

                if (currentTeam == null)
                    throw new InvalidOperationException($"Team {currentTeamId} not found.");

                var lineup = await _teamPlanService.GetStartingLineupAsync(currentTeam, includeDetails: false);
                var outfieldPlayers = lineup.Where(p => p.Position.Code != "GK").ToList();

                if (!outfieldPlayers.Any())
                    throw new InvalidOperationException($"No outfield players found for team {currentTeam.Id}");

                var player = outfieldPlayers[_random.Next(outfieldPlayers.Count)];

                var eventType = _matchEventService.GetRandomEvent();
                var outcome = _matchEventService.GetEventOutcome(player, eventType);

                if (eventType.Code == "SHT" && outcome.Name == "Goal")
                {
                    UpdateFixtureScore(dbFixture, currentTeamId);
                }

                var playerStatsMap = match.PlayerStats.ToDictionary(s => s.PlayerId);
                if (playerStatsMap.TryGetValue(player.Id, out var stats))
                {
                    _playerStatsService.UpdateStats(new MatchEvent
                    {
                        Minute = match.CurrentMinute,
                        Player = player,
                        Team = currentTeam,
                        TeamId = currentTeam.Id,
                        EventType = eventType,
                        EventTypeId = eventType.Id,
                        Outcome = outcome
                    }, stats);
                }

                // 💾 Ново: сейвваме прогреса на всеки няколко стъпки (например на всеки 5)
                if (eventCount % 5 == 0)
                {
                    await _matchService.SaveMatchProgressAsync(match);
                }

                ChangeTurn(match);
                eventCount++;
            }

            await EndMatch(match);
            await _matchService.SaveMatchProgressAsync(match); // 💾 крайно сейвване

            var fixtureAfter = match.Fixture ?? dbFixture;
            bool isKnockout = fixtureAfter.IsElimination
                              || (fixtureAfter.EuropeanCupPhase?.PhaseTemplate?.IsKnockout == true);

            if (isKnockout && fixtureAfter.WinnerTeamId == null)
            {
                await HandlePenaltyShootoutAsync(match);
            }

            await _standingsDispatcher.UpdateAfterMatchAsync(match.Fixture);

            sw.Stop();
            return match;
        }

        private void UpdateFixtureScore(Fixture fixture, int? currentTeamId)
        {
            if (currentTeamId == fixture.HomeTeamId)
            {
                fixture.HomeTeamGoals = (fixture.HomeTeamGoals ?? 0) + 1;
            }
            else if (currentTeamId == fixture.AwayTeamId)
            {
                fixture.AwayTeamGoals = (fixture.AwayTeamGoals ?? 0) + 1;
            }
        }
        private async Task HandlePenaltyShootoutAsync(Match match)
        {
            await _penaltyService.RunPenaltyShootoutAsync(match);

        }
        public async Task RunMatch(Match match)
        {
            await SimulateMatchAsync(match.Fixture, match.Fixture.GameSave);
        }

       
    }
}
