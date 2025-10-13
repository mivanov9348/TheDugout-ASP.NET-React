namespace TheDugout.Services.MatchEngine
{
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Services.League;
    using TheDugout.Services.Match;
    using TheDugout.Services.Player;
    using TheDugout.Services.Standings;
    using TheDugout.Services.Team;
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
        public async Task StartMatch(Models.Matches.Match match)
        {
            match.Status = MatchStatus.Live;
            match.CurrentMinute = 0;
            match.Fixture.HomeTeamGoals = 0;
            match.Fixture.AwayTeamGoals = 0;
            match.CurrentTurn = MatchTurn.Home;

            if (match.PlayerStats == null || !match.PlayerStats.Any())
                match.PlayerStats = await _playerStatsService.InitializeMatchStatsAsync(match);
        }
        public async Task EndMatch(Models.Matches.Match match)
        {
            match.Status = MatchStatus.Played;

            var fixture = match.Fixture;
            fixture.Status = FixtureStatusEnum.Played;

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
                await _playerStatsService.UpdateSeasonStatsAfterMatchAsync(match);
            }

        }
        public void PlayNextMinute(Models.Matches.Match match)
        {
            int increment = _random.Next(1, 6); // по-малки скокове за по-реалистично
            match.CurrentMinute += increment;
        }
        public void ChangeTurn(Models.Matches.Match match)
        {
            match.CurrentTurn = match.CurrentTurn == MatchTurn.Home
                ? MatchTurn.Away
                : MatchTurn.Home;
        }
        public bool IsMatchFinished(Models.Matches.Match match)
        {
            return match.CurrentMinute >= 90;
        }
        public async Task<MatchEvent?> PlayStep(Models.Matches.Match match)
        {
            if (IsMatchFinished(match))
            {
                EndMatch(match);
                return null;
            }

            // 1. Минутка
            PlayNextMinute(match);

            // 2. Определи отбора в атака
            var currentTeamId = match.CurrentTurn == MatchTurn.Home
                ? match.Fixture.HomeTeamId
                : match.Fixture.AwayTeamId;

            var currentTeam = _context.Teams.FirstOrDefault(t => t.Id == currentTeamId)
                ?? throw new InvalidOperationException($"Team {currentTeamId} not found.");

            var lineup = await _teamPlanService.GetStartingLineupAsync(currentTeam);
            var outfieldPlayers = lineup.Where(p => p.Position.Code != "GK").ToList();
            if (!outfieldPlayers.Any())
                throw new InvalidOperationException($"No outfield players found for team {currentTeam.Id}");

            var player = outfieldPlayers[_random.Next(outfieldPlayers.Count)];

            // 3. Евент
            var eventType = _matchEventService.GetRandomEvent();
            var outcome = _matchEventService.GetEventOutcome(player, eventType);
            var commentary = _matchEventService.GetRandomCommentary(outcome, player);

            var matchEvent = await _matchEventService.CreateMatchEvent(match.Id, match.CurrentMinute, currentTeam, player, eventType, outcome, commentary);

            UpdateFixtureScore(match, currentTeam.Id, player, eventType, outcome);

            // 4. Ъпдейт на статистики
            var playerStats = match.PlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
            if (playerStats != null)
                _playerStatsService.UpdateStats(matchEvent, playerStats);

            // 5. Смяна на притежание
            ChangeTurn(match);

            // 6. Ако вече е минало 90' → край
            if (IsMatchFinished(match))
            {
                await EndMatch(match);
                await _standingsDispatcher.UpdateAfterMatchAsync(match.Fixture);
            }

            return matchEvent;
        }
        public async Task<Models.Matches.Match> SimulateMatchAsync(Models.Fixtures.Fixture fixture, GameSave gameSave)
        {
            if (fixture is null) throw new ArgumentNullException(nameof(fixture));
            if (gameSave is null) throw new ArgumentNullException(nameof(gameSave));

            var sw = Stopwatch.StartNew(); // 🕒 стартираме таймера

            var dbFixture = await _context.Fixtures
                .Include(f => f.HomeTeam).ThenInclude(t => t.Players)
                .Include(f => f.AwayTeam).ThenInclude(t => t.Players)
                .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(f => f.Id == fixture.Id);

            if (dbFixture is null)
                throw new InvalidOperationException($"Fixture with Id {fixture.Id} not found in DB.");

            var match = await _matchService.CreateMatchFromFixtureAsync(dbFixture, gameSave);
            match.PlayerStats = await _playerStatsService.EnsureMatchStatsAsync(match);

            int eventCount = 0; // 🧮 броим събитията

            while (!IsMatchFinished(match))
            {
                PlayNextMinute(match);

                var currentTeamId = match.CurrentTurn == MatchTurn.Home
                    ? match.Fixture.HomeTeamId
                    : match.Fixture.AwayTeamId;

                var currentTeam = _context.Teams.FirstOrDefault(t => t.Id == currentTeamId)
                    ?? throw new InvalidOperationException($"Team {currentTeamId} not found.");

                var lineup = await _teamPlanService.GetStartingLineupAsync(currentTeam);
                var outfieldPlayers = lineup.Where(p => p.Position.Code != "GK").ToList();
                if (!outfieldPlayers.Any())
                    throw new InvalidOperationException($"No outfield players found for team {currentTeam.Id}");

                var player = outfieldPlayers[_random.Next(outfieldPlayers.Count)];

                var eventType = _matchEventService.GetRandomEvent();
                var outcome = _matchEventService.GetEventOutcome(player, eventType);

                if (eventType.Code == "SHT" && outcome.Name == "Goal")
                {
                    UpdateFixtureScore(match, currentTeamId, player, eventType, outcome);
                }

                var playerStats = match.PlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
                if (playerStats != null)
                {
                    _playerStatsService.UpdateStats(new Models.Matches.MatchEvent
                    {
                        Minute = match.CurrentMinute,
                        Player = player,
                        Team = currentTeam,
                        TeamId = currentTeam.Id,
                        EventType = eventType,
                        Outcome = outcome
                    }, playerStats);
                }

                ChangeTurn(match);
                eventCount++;
            }

            await EndMatch(match);

            var fixtureAfter = match.Fixture ?? dbFixture;
            bool isKnockout = fixtureAfter.IsElimination
                              || (fixtureAfter.EuropeanCupPhase?.PhaseTemplate?.IsKnockout == true);

            if (isKnockout && fixtureAfter.WinnerTeamId == null)
            {
                await HandlePenaltyShootoutAsync(match);
            }

            await _standingsDispatcher.UpdateAfterMatchAsync(match.Fixture);

            sw.Stop(); // 🛑 спираме таймера

            Console.WriteLine(
                $"[SIMULATE] {fixture.HomeTeam?.Name} vs {fixture.AwayTeam?.Name} " +
                $"→ {fixture.HomeTeamGoals}-{fixture.AwayTeamGoals} " +
                $"({sw.ElapsedMilliseconds} ms, {eventCount} events)"
            );

            return match;
        }


        private void UpdateFixtureScore(Models.Matches.Match match, int? currentTeamId, Models.Players.Player player, EventType eventType, EventOutcome outcome)
        {
            if (eventType.Code == "SHT" && outcome.Name == "Goal")
            {
                if (currentTeamId == match.Fixture.HomeTeamId)
                {
                    match.Fixture.HomeTeamGoals = (match.Fixture.HomeTeamGoals ?? 0) + 1;
                }
                else if (currentTeamId == match.Fixture.AwayTeamId)
                {
                    match.Fixture.AwayTeamGoals = (match.Fixture.AwayTeamGoals ?? 0) + 1;
                }
            }
        }
        private async Task HandlePenaltyShootoutAsync(Models.Matches.Match match)
        {
            match = await _penaltyService.RunPenaltyShootoutAsync(match);

            if (match.Fixture.HomeTeamGoals > match.Fixture.AwayTeamGoals)
                match.Fixture.WinnerTeamId = match.Fixture.HomeTeamId;
            else
                match.Fixture.WinnerTeamId = match.Fixture.AwayTeamId;
        }
        public async Task RunMatch(Models.Matches.Match match)
        {
            StartMatch(match);
            while (!IsMatchFinished(match))
                await PlayStep(match);

            await EndMatch(match);
        }
    }
}
