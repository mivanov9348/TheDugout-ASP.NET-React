namespace TheDugout.Services.MatchEngine
{
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Services.Facilities;
    using TheDugout.Services.GameSettings.Interfaces;
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
        private readonly IPlayerGenerationService _playerGenerationService;
        private readonly ILeagueStandingsService _leagueStandingsService;
        private readonly IStandingsDispatcherService _standingsDispatcher;
        private readonly IPenaltyShootoutService _penaltyService;
        private readonly IMoneyPrizeService _moneyPrizeService;
        private readonly IStadiumService _stadiumService;
        private readonly ITeamGenerationService _teamGeneratorService;
        private readonly ILogger<MatchEngine> _logger;
        private readonly DugoutDbContext _context;

        public MatchEngine(
            ITeamPlanService teamPlanService,
            IMatchEventService matchEventService,
            IPlayerStatsService playerStatsService,
            IPlayerGenerationService playerGenerationService,
            ILeagueStandingsService leagueStandingsService,
            IStandingsDispatcherService standingsDispatcher,
            IMatchService matchService,
            IPenaltyShootoutService penaltyService,
            IMoneyPrizeService moneyPrizeService,
            IStadiumService stadiumService,
            ITeamGenerationService teamGeneratorService,
            ILogger<MatchEngine> logger,
            DugoutDbContext context)
        {
            _teamPlanService = teamPlanService;
            _matchEventService = matchEventService;
            _playerStatsService = playerStatsService;
            _playerGenerationService = playerGenerationService;
            _leagueStandingsService = leagueStandingsService;
            _matchService = matchService;
            _standingsDispatcher = standingsDispatcher;
            _penaltyService = penaltyService;
            _moneyPrizeService = moneyPrizeService;
            _stadiumService = stadiumService;
            _teamGeneratorService = teamGeneratorService;
            _logger = logger;
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
                await _playerStatsService.UpdateCompetitionStatsAfterMatchAsync(match);
                await _playerStatsService.UpdateSeasonStatsAfterMatchAsync(match);

                foreach (var ps in match.PlayerStats)
                {
                    var player = ps.Player;
                    if (player == null) continue;

                    // Зареждаме текущия сезонен запис
                    var stats = await _context.PlayerSeasonStats
                        .Where(s => s.PlayerId == player.Id)
                        .OrderByDescending(s => s.SeasonId)
                        .FirstOrDefaultAsync();

                    if (stats != null)
                    {
                        // 🧠 Актуализирай текущия ability
                        _playerGenerationService.UpdateCurrentAbility(player, stats);

                        // 💰 Пресметни новата цена
                        await _playerGenerationService.UpdatePlayerPriceAsync(player);
                    }
                }
                await _context.SaveChangesAsync();

            }

            await GrantMatchPrizesAsync(match, fixture);
        }
        public void PlayNextMinute(Match match)
        {
            int increment = _random.Next(1, 11);
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

            var totalSw = Stopwatch.StartNew();
            _logger.LogInformation("▶️ Starting simulation for {Home} vs {Away}", fixture.HomeTeam?.Name, fixture.AwayTeam?.Name);

            var sw = Stopwatch.StartNew();
            var dbFixture = await _context.Fixtures
                .Include(f => f.HomeTeam).ThenInclude(t => t.Players)
                .Include(f => f.AwayTeam).ThenInclude(t => t.Players)
                .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.PhaseTemplate)
                .FirstOrDefaultAsync(f => f.Id == fixture.Id);
            sw.Stop();
            _logger.LogInformation("⏱ Loaded fixture in {Elapsed} ms", sw.ElapsedMilliseconds);

            if (dbFixture is null) throw new InvalidOperationException($"Fixture {fixture.Id} not found.");

            await _teamPlanService.AutoPickTacticAsync(dbFixture.HomeTeam.Id, gameSave.Id);
            await _teamPlanService.AutoPickTacticAsync(dbFixture.AwayTeam.Id, gameSave.Id);
            _logger.LogInformation("🔁 Auto tactics reinitialized for both teams before match");

            sw.Restart();
            var match = await _matchService.GetOrCreateMatchAsync(fixture, gameSave);
            await _matchService.GenerateAttendanceAsync(match);
            var revenue = await _stadiumService.GenerateMatchRevenueAsync(match);
            _logger.LogInformation($"revenue from match: {revenue}");

            match.PlayerStats = await _playerStatsService.EnsureMatchStatsAsync(match);
            sw.Stop();
            _logger.LogInformation("🔧 Created/Loaded match + stats in {Elapsed} ms", sw.ElapsedMilliseconds);

            // Кеширане на lineup-и
            var homeLineup = (await _teamPlanService.GetStartingLineupAsync(dbFixture.HomeTeam, false))
                                .Where(p => p.Position.Code != "GK").ToList();
            var awayLineup = (await _teamPlanService.GetStartingLineupAsync(dbFixture.AwayTeam, false))
                                .Where(p => p.Position.Code != "GK").ToList();

            int eventCount = 0;

            while (!IsMatchFinished(match))
            {
                var loopSw = Stopwatch.StartNew();
                PlayNextMinute(match);

                var currentTeam = match.CurrentTurn == MatchTurn.Home
                    ? dbFixture.HomeTeam
                    : dbFixture.AwayTeam;
                var outfieldPlayers = match.CurrentTurn == MatchTurn.Home ? homeLineup : awayLineup;

                var playerSw = Stopwatch.StartNew();
                var player = outfieldPlayers[_random.Next(outfieldPlayers.Count)];
                playerSw.Stop();

                var eventSw = Stopwatch.StartNew();
                var eventType = _matchEventService.GetRandomEvent();
                var outcome = _matchEventService.GetEventOutcome(player, eventType);
                eventSw.Stop();

                if (eventType.Code == "SHT" && outcome.Name == "Goal")
                {
                    UpdateFixtureScore(dbFixture, currentTeam.Id);
                    _logger.LogInformation("⚽ GOAL! {Team} scores at minute {Minute}", currentTeam.Name, match.CurrentMinute);
                }

                var statsSw = Stopwatch.StartNew();
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
                statsSw.Stop();

                if (eventCount % 15 == 0)
                {
                    sw.Restart();
                    await _matchService.SaveMatchProgressAsync(match);
                    sw.Stop();
                    _logger.LogDebug("💾 Saved progress in {Elapsed} ms", sw.ElapsedMilliseconds);
                }

                ChangeTurn(match);
                eventCount++;
                loopSw.Stop();

                if (loopSw.ElapsedMilliseconds > 50)
                    _logger.LogWarning("Minute {Minute} took {Elapsed} ms", match.CurrentMinute, loopSw.ElapsedMilliseconds);
            }

            sw.Restart();
            await EndMatch(match);
            await _matchService.SaveMatchProgressAsync(match);
            sw.Stop();
            _logger.LogInformation("🏁 EndMatch + save in {Elapsed} ms", sw.ElapsedMilliseconds);

            if ((dbFixture.IsElimination || dbFixture.EuropeanCupPhase?.PhaseTemplate?.IsKnockout == true)
                && dbFixture.WinnerTeamId == null)
            {
                sw.Restart();
                await HandlePenaltyShootoutAsync(match);
                sw.Stop();
                _logger.LogInformation("🏆 Penalty shootout processed in {Elapsed} ms", sw.ElapsedMilliseconds);
            }

            sw.Restart();
            await _standingsDispatcher.UpdateAfterMatchAsync(match.Fixture);
            sw.Stop();
            _logger.LogInformation("📊 Standings updated in {Elapsed} ms", sw.ElapsedMilliseconds);

            totalSw.Stop();
            _logger.LogInformation("✅ Finished match in {Elapsed} ms ({Seconds:N2}s)", totalSw.ElapsedMilliseconds, totalSw.Elapsed.TotalSeconds);

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
        private async Task GrantMatchPrizesAsync(Match match, Fixture fixture)
        {
            var gameSave = fixture.GameSave ?? throw new InvalidOperationException("Fixture.GameSave is null.");

            if (fixture.WinnerTeamId == null)
            {
                await _moneyPrizeService.GrantToTeamAsync(gameSave, "MATCH_DRAW", fixture.HomeTeam);
                await _moneyPrizeService.GrantToTeamAsync(gameSave, "MATCH_DRAW", fixture.AwayTeam);
                await _teamGeneratorService.UpdatePopularityAsync(fixture.HomeTeam, TeamEventType.Draw);
                await _teamGeneratorService.UpdatePopularityAsync(fixture.AwayTeam, TeamEventType.Draw);
            }
            else if (fixture.WinnerTeamId == fixture.HomeTeamId)
            {
                await _moneyPrizeService.GrantToTeamAsync(gameSave, "MATCH_WIN", fixture.HomeTeam);
                await _teamGeneratorService.UpdatePopularityAsync(fixture.HomeTeam, TeamEventType.Win);

            }
            else if (fixture.WinnerTeamId == fixture.AwayTeamId)
            {
                await _moneyPrizeService.GrantToTeamAsync(gameSave, "MATCH_WIN", fixture.AwayTeam);
                await _teamGeneratorService.UpdatePopularityAsync(fixture.AwayTeam, TeamEventType.Win);

            }
        }

    }
}
