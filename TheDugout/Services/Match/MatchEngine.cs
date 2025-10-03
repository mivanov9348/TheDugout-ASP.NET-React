using TheDugout.Data;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Services.League;
using TheDugout.Services.Match;
using TheDugout.Services.Player;
using TheDugout.Services.Standings;
using TheDugout.Services.Team;

namespace TheDugout.Services.MatchEngine
{
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
        public void StartMatch(Models.Matches.Match match)
        {
            match.Status = MatchStatus.Live;
            match.CurrentMinute = 0;
            match.Fixture.HomeTeamGoals = 0;
            match.Fixture.AwayTeamGoals = 0;
            match.CurrentTurn = MatchTurn.Home;

            // инициализация на статистиките за играчите
            match.PlayerStats = _playerStatsService.InitializeMatchStats(match);
        }
        public async void EndMatch(Models.Matches.Match match)
        {
            match.Status = MatchStatus.Played;

            var fixture = match.Fixture;
            fixture.Status = FixtureStatus.Played;

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

                if (fixture.IsElimination)
                {
                    await HandlePenaltyShootoutAsync(match);
                }
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

            var matchEvent = _matchEventService.CreateMatchEvent(
                match.Id, match.CurrentMinute, currentTeam, player, eventType, outcome, commentary);

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
                EndMatch(match);
                await _standingsDispatcher.UpdateAfterMatchAsync(match.Fixture);
            }

            return matchEvent;
        }
        public async Task<Models.Matches.Match> SimulateMatchAsync(Models.Fixtures.Fixture fixture, GameSave gameSave)
        {
            if (fixture == null) throw new ArgumentNullException(nameof(fixture));
            if (gameSave == null) throw new ArgumentNullException(nameof(gameSave));

            // 1. Създай мач от фикстура
            var match = await _matchService.CreateMatchFromFixtureAsync(fixture, gameSave);

            // 2. Върти докато не свърши
            while (!IsMatchFinished(match))
            {
                // 2.1 Минутка
                PlayNextMinute(match);

                // 2.2 Определи кой отбор е на ход
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

                // 2.3 Евент + изход (но без да създаваме MatchEvent)
                var eventType = _matchEventService.GetRandomEvent();
                var outcome = _matchEventService.GetEventOutcome(player, eventType);

                // 2.4 Update на статистики + резултат
                if (eventType.Code == "SHT" && outcome.Name == "Goal")
                {
                    UpdateFixtureScore(match, currentTeamId, player, eventType, outcome);
                }

                var playerStats = match.PlayerStats.FirstOrDefault(s => s.PlayerId == player.Id);
                if (playerStats != null)
                    _playerStatsService.UpdateStats(new Models.Matches.MatchEvent
                    {
                        Minute = match.CurrentMinute,
                        Player = player,
                        Team = currentTeam,
                        TeamId = currentTeamId,
                        EventType = eventType,
                        Outcome = outcome
                    }, playerStats);

                // 2.5 Смяна на притежание
                ChangeTurn(match);
            }

            // 3. Край на мача
            EndMatch(match);
            await _standingsDispatcher.UpdateAfterMatchAsync(match.Fixture);

            return match;
        }
        private void UpdateFixtureScore(Models.Matches.Match match, int? currentTeamId, Models.Players.Player player, EventType eventType, EventOutcome outcome)
        {
            if (eventType.Code == "SHT" && outcome.Name == "Goal")
            {
                if (currentTeamId == match.Fixture.HomeTeamId)
                {
                    match.Fixture.HomeTeamGoals = (match.Fixture.HomeTeamGoals ?? 0) + 1;
                    Console.WriteLine($"GOAL! {match.Fixture.HomeTeam?.Name} scores! {match.Fixture.HomeTeamGoals}-{match.Fixture.AwayTeamGoals}"); // 👈 ДОБАВИ ЛОГ
                }
                else if (currentTeamId == match.Fixture.AwayTeamId)
                {
                    match.Fixture.AwayTeamGoals = (match.Fixture.AwayTeamGoals ?? 0) + 1;
                    Console.WriteLine($"GOAL! {match.Fixture.AwayTeam?.Name} scores! {match.Fixture.HomeTeamGoals}-{match.Fixture.AwayTeamGoals}"); // 👈 ДОБАВИ ЛОГ
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

            EndMatch(match);
        }
    }
}
