namespace TheDugout.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Text.Json;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Services.Match.Interfaces;
    using TheDugout.Services.Player.Interfaces;

    [ApiController]
    [Route("api/matches")]
    public class MatchesController : ControllerBase
    {
        private readonly DugoutDbContext _context;
        private readonly IMatchService _matchService;
        private readonly IMatchEngine _matchEngine;
        private readonly IPlayerStatsService _playerStatsService;
        private readonly IMatchResponseService _matchResponseService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(
            DugoutDbContext context,
            IMatchService matchService,
            IMatchEngine matchEngine,
            IPlayerStatsService playerStatsService,
            IMatchResponseService matchResponseService,
            ILogger<MatchesController> logger)
        {
            _context = context;
            _matchService = matchService;
            _matchEngine = matchEngine;
            _playerStatsService = playerStatsService;
            _matchResponseService = matchResponseService;
            _logger = logger;
        }

        [HttpGet("today/{gameSaveId}")]
        public async Task<IActionResult> GetTodayMatches(int gameSaveId)
        {
            var save = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(ut => ut.League)
                        .ThenInclude(l => l.Template)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(ut => ut.CupTeams)
                        .ThenInclude(cp => cp.Cup)
                            .ThenInclude(c => c.Template)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (save == null)
                return NotFound();

            var activeSeason = _context.Seasons.FirstOrDefault(x => x.GameSaveId == gameSaveId && x.IsActive == true);

            var today = activeSeason.CurrentDate.Date;

            var fixtures = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.League).ThenInclude(l => l.Template)
                .Include(f => f.CupRound).ThenInclude(cr => cr.Cup).ThenInclude(c => c.Template)
                .Include(f => f.EuropeanCupPhase).ThenInclude(p => p.EuropeanCup).ThenInclude(ec => ec.Template)
                .Include(f => f.Match).ThenInclude(m => m.Penalties)
                .Where(f => f.GameSaveId == gameSaveId && f.Date.Date == today)
                .ToListAsync();

            // 🧩 Определяме "основната" конкуренция на user team
            string? userCompetitionName = null;

            if (save.UserTeam?.League?.Template != null)
                userCompetitionName = save.UserTeam.League.Template.Name;
            else if (save.UserTeam?.CupTeams?.Any() == true)
                userCompetitionName = save.UserTeam.CupTeams
                    .FirstOrDefault()?.Cup?.Template?.Name;

            // ⚡️ Сортираме — първо мачовете от същата лига/купа като на UserTeam
            fixtures = fixtures
                .OrderByDescending(f =>
                    (f.League != null && f.League.Template.Name == userCompetitionName) ||
                    (f.CupRound != null && f.CupRound.Cup.Template.Name == userCompetitionName) ||
                    (f.EuropeanCupPhase != null && f.EuropeanCupPhase.EuropeanCup.Template.Name == userCompetitionName)
                )
                .ThenBy(f => f.League?.Template?.Name ?? f.CupRound?.Cup?.Template?.Name ?? f.EuropeanCupPhase?.EuropeanCup?.Template?.Name ?? "")
                .ToList();

            var matches = await _matchResponseService.GetFormattedMatchesResponseAsync(fixtures, save);

            return Ok(new { matches });
        }

        [HttpPost("simulate/{gameSaveId}")]
        public async Task<IActionResult> SimulateMatches(int gameSaveId)
        {
            var gameSave = await _context.GameSaves
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                    .ThenInclude(ut => ut.League)
                        .ThenInclude(l => l.Template)
                    .Include(gs => gs.Competitions)

                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (gameSave == null)
                return NotFound($"GameSave {gameSaveId} not found.");

            var currentSeason = gameSave.Seasons
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            if (currentSeason == null)
                return BadRequest("No active season found for this GameSave.");

            var currentDate = currentSeason.CurrentDate.Date;

            // *** ПРОМЯНА: Зареждаме мачовете ефективно и с всички филтри ***
            var fixtures = await _context.Fixtures
                .Where(f => f.GameSaveId == gameSaveId &&
                            f.Date.Date == currentDate &&
                            f.SeasonId == currentSeason.Id &&
                            f.Status != MatchStageEnum.Played) // Важно за идемпотентност
                .ToListAsync();

            if (!fixtures.Any())
                return Ok(new { message = "No fixtures to simulate." }); 

            if (gameSave.UserTeamId.HasValue)
            {
                var userTeamId = gameSave.UserTeamId.Value;

                var userFixturesToday = fixtures
                    .Where(f => f.HomeTeamId == userTeamId || f.AwayTeamId == userTeamId)
                    .ToList();

                if (userFixturesToday.Any())
                {
                    bool hasTactic = await _context.TeamTactics
                        .AnyAsync(tt => tt.TeamId == userTeamId && tt.GameSaveId == gameSaveId);

                    if (!hasTactic)
                    {
                        return BadRequest("❌ Your team has a scheduled match today but no tactic is saved. Please create one before simulating.");
                    }
                }
            }

            // *** ПРОМЯНА: Обвиваме всичко в транзакция ***
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                

                foreach (var fixture in fixtures)
                {
                    var match = fixture.Match;

                    if (match == null)
                    {
                        // Предполагаме, че GetOrCreateMatchAsync НЕ запазва, 
                        // а само добавя към _context
                        match = await _matchService.GetOrCreateMatchAsync(fixture, gameSave);
                        fixture.Match = match;
                    }

                    // SimulateMatchAsync вече НЕ запазва промени
                    await _matchEngine.SimulateMatchAsync(fixture, gameSave);

                    // Маркираме статусите
                    match.Status = MatchStageEnum.Played;
                    fixture.Status = MatchStageEnum.Played;
                }

                // *** ПРОМЯНА: Запазваме ВСИЧКО наведнъж ***
                // Това включва: резултати, голове, статистики, 
                // промени по LeagueStandings И новите статуси на Fixture/Match.
                await _context.SaveChangesAsync();

                // *** ПРОМЯНА: Потвърждаваме транзакцията ***
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // *** ПРОМЯНА: Ако нещо гръмне, връщаме всичко назад ***
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Failed to simulate matches. Transaction rolled back.");

                // Върни грешката, която хвана (напр. "Standings not initialized")
                return StatusCode(500, $"Error during simulation: {ex.Message}");
            }

            _context.ChangeTracker.Clear();

            // ... (Останалата част от твоя код за връщане на Ok() с данните е същата) ...
            // ... (Зареждаш updatedSave, todayMatches, и т.н.) ...

            // Само за пълнота, ето пример как би изглеждало:
            var updatedSave = await _context.GameSaves
               .Include(gs => gs.Seasons)
               .Include(gs => gs.UserTeam)
                   .ThenInclude(ut => ut.League)
                       .ThenInclude(l => l.Template)
               .AsNoTracking() // Добра практика след SaveChanges/Clear
               .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            var updatedSeason = updatedSave.Seasons.FirstOrDefault(s => s.Id == currentSeason.Id);
            var today = updatedSeason?.CurrentDate.Date ?? DateTime.Today;

            var todayMatches = await _context.Fixtures
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.League).ThenInclude(l => l.Template)
                .Include(f => f.CupRound).ThenInclude(cr => cr.Cup).ThenInclude(c => c.Template)
                .Include(f => f.EuropeanCupPhase).ThenInclude(ecp => ecp.EuropeanCup).ThenInclude(ec => ec.Template)
                .Include(f => f.Match).ThenInclude(m => m.Penalties)
                .Where(f => f.GameSaveId == gameSaveId && f.Date.Date == today)
                .AsNoTracking()
                .ToListAsync();

            var hasUnplayedMatchesToday = todayMatches.Any(m => m.Status == 0);
            var hasMatchesToday = todayMatches.Any();

            var matches = await _matchResponseService.GetFormattedMatchesResponseAsync(todayMatches, updatedSave);

            return Ok(new
            {
                message = "Matches simulated successfully",
                gameStatus = new
                {
                    gameSave = new
                    {
                        updatedSave.Id,
                        updatedSave.UserTeamId,
                        UserTeam = new
                        {
                            updatedSave.UserTeam.Name,
                            updatedSave.UserTeam.Balance,
                            LeagueName = updatedSave.UserTeam.League.Template.Name
                        },
                        Seasons = updatedSave.Seasons.Select(s => new
                        {
                            s.Id,
                            s.CurrentDate
                        }),
                    },
                    hasUnplayedMatchesToday,
                    hasMatchesToday
                },
                matches
            });
        }

        [HttpGet("simulate-stream/{gameSaveId}")]
        public async Task SimulateMatchesStream(int gameSaveId)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            var gameSave = await _context.GameSaves
                .Include(gs => gs.Fixtures)
                    .ThenInclude(f => f.Match)
                .Include(gs => gs.Seasons)
                .Include(gs => gs.UserTeam)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);

            if (gameSave == null)
            {
                await Response.WriteAsync("data: {\"error\":\"GameSave not found\"}\n\n");
                await Response.Body.FlushAsync();
                return;
            }

            var currentSeason = gameSave.Seasons
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            if (currentSeason == null)
            {
                await Response.WriteAsync("data: {\"error\":\"No active season\"}\n\n");
                await Response.Body.FlushAsync();
                return;
            }

            var currentDate = currentSeason.CurrentDate.Date;
            var fixtures = gameSave.Fixtures
                .Where(f => f.Date.Date == currentDate)
                .ToList();

            if (!fixtures.Any())
            {
                await Response.WriteAsync("data: {\"message\":\"No fixtures to simulate.\"}\n\n");
                await Response.Body.FlushAsync();
                return;
            }

            if (gameSave.UserTeamId.HasValue)
            {
                var userTeamId = gameSave.UserTeamId.Value;

                var userFixturesToday = fixtures
                    .Where(f => f.HomeTeamId == userTeamId || f.AwayTeamId == userTeamId)
                    .ToList();

                if (userFixturesToday.Any())
                {
                    bool hasTactic = await _context.TeamTactics
                        .AnyAsync(tt => tt.TeamId == userTeamId && tt.GameSaveId == gameSaveId);

                    if (!hasTactic)
                    {
                        var errorJson = JsonSerializer.Serialize(new
                        {
                            error = "❌ Your team has a scheduled match today but no tactic is saved. Please create one before simulating."
                        });
                        await Response.WriteAsync($"data: {errorJson}\n\n");
                        await Response.Body.FlushAsync();
                        return;
                    }
                }
            }

            foreach (var fixture in fixtures)
            {
                var match = fixture.Match ?? await _matchService.GetOrCreateMatchAsync(fixture, gameSave);
                fixture.Match = match;

                await _matchEngine.SimulateMatchAsync(fixture, gameSave);

                match.Status = MatchStageEnum.Played;
                fixture.Status = MatchStageEnum.Played;

                await _context.SaveChangesAsync();

                var competitionName = GetCompetitionDisplayName(fixture);
                var homeName = fixture.HomeTeam?.Name ?? "Home";
                var awayName = fixture.AwayTeam?.Name ?? "Away";
                var homeGoals = fixture.HomeTeamGoals ?? 0;
                var awayGoals = fixture.AwayTeamGoals ?? 0;

                var message = $"🏆 {competitionName}: {homeName} - {awayName} {homeGoals}:{awayGoals}";

                var json = JsonSerializer.Serialize(new
                {
                    message,
                    match = new
                    {
                        fixture.Id,
                        Competition = competitionName,
                        Home = homeName,
                        Away = awayName,
                        HomeGoals = homeGoals,
                        AwayGoals = awayGoals,
                        fixture.Date
                    }
                });

                // 🟢 изпращаме резултата от този мач
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();

                await Task.Delay(500); // малка пауза между мачовете
            }

            // ✅ изпращаме "done" чак след последния мач
            await Response.WriteAsync("data: {\"message\":\"done\"}\n\n");
            await Response.Body.FlushAsync();
        }

        [HttpGet("{fixtureId}")]
        public async Task<IActionResult> GetMatchDetails(int fixtureId)
        {
            _logger.LogInformation("==== GetMatchDetails called with matchId={MatchId} ====", fixtureId);

            var match = await _context.Matches
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                .Include(m => m.PlayerStats)
                    .ThenInclude(ps => ps.Player)
                .Include(m => m.Events)
                    .ThenInclude(e => e.EventType)
                .Include(m => m.Events)
                    .ThenInclude(e => e.Player)
                .Include(m => m.Competition)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.FixtureId == fixtureId);

            if (match == null)
            {
                _logger.LogWarning("Match {MatchId} not found", match.Id);
                return NotFound();
            }

            var fixture = match.Fixture;
            if (fixture == null)
            {
                _logger.LogWarning("Match {MatchId} has no fixture", match.Id);
                return BadRequest("Match has no fixture assigned.");
            }

            var goalScorers = match.PlayerStats
                .Where(ps => ps.Goals > 0)
                .Select(ps => new
                {
                    TeamId = ps.Player.TeamId,
                    PlayerId = ps.PlayerId,
                    Scorer = $"{ps.Player.FirstName} {ps.Player.LastName}",
                    Goals = ps.Goals
                })
                .ToList();

            var goalEvents = match.Events
                .Where(e => e.EventType.Code.Equals("GOAL", StringComparison.OrdinalIgnoreCase)
                         || e.EventType.Code.Equals("G", StringComparison.OrdinalIgnoreCase))
                .Select(e => new { e.TeamId, e.PlayerId, e.Minute })
                .ToList();

            List<object> BuildGoals(int? teamId)
            {
                var teamGoals = new List<object>();

                foreach (var scorer in goalScorers.Where(s => s.TeamId == teamId))
                {
                    var minutes = goalEvents
                        .Where(g => g.TeamId == teamId && g.PlayerId == scorer.PlayerId)
                        .Select(g => (int?)g.Minute)
                        .OrderBy(m => m)
                        .ToList();

                    if (!minutes.Any())
                        minutes.AddRange(Enumerable.Repeat<int?>(null, scorer.Goals));

                    for (int i = 0; i < scorer.Goals; i++)
                    {
                        var minute = i < minutes.Count ? minutes[i] : (int?)null;
                        teamGoals.Add(new { minute, scorer = scorer.Scorer, playerId = scorer.PlayerId });
                    }
                }

                return teamGoals.OrderBy(g => ((dynamic)g).minute ?? 999).ToList();
            }

            var homeGoals = BuildGoals(fixture.HomeTeamId);
            var awayGoals = BuildGoals(fixture.AwayTeamId);

            var dto = new
            {
                id = match.Id,
                date = fixture.Date,
                status = match.Status.ToString(),
                currentMinute = match.CurrentMinute,
                result = $"{fixture.HomeTeamGoals ?? 0} - {fixture.AwayTeamGoals ?? 0}",
                winner = fixture.WinnerTeamId == fixture.HomeTeamId
                    ? fixture.HomeTeam?.Name
                    : fixture.WinnerTeamId == fixture.AwayTeamId
                        ? fixture.AwayTeam?.Name
                        : null,
                homeTeam = new
                {
                    name = fixture.HomeTeam?.Name ?? "Home Team",
                    logo = string.IsNullOrWhiteSpace(fixture.HomeTeam?.LogoFileName)
                        ? null
                        : $"/uploads/logos/{fixture.HomeTeam.LogoFileName}",
                    goals = homeGoals
                },
                awayTeam = new
                {
                    name = fixture.AwayTeam?.Name ?? "Away Team",
                    logo = string.IsNullOrWhiteSpace(fixture.AwayTeam?.LogoFileName)
                        ? null
                        : $"/uploads/logos/{fixture.AwayTeam.LogoFileName}",
                    goals = awayGoals
                }
            };

            _logger.LogInformation(
                "Returning match DTO with {HomeGoals}-{AwayGoals} goals",
                fixture.HomeTeamGoals, fixture.AwayTeamGoals
            );

            // 🔹 Логваме какво точно се връща към фронтенда (в JSON формат)
            var dtoJson = System.Text.Json.JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogInformation("=== Sending DTO to frontend ===\n{DtoJson}", dtoJson);

            return Ok(dto);
        }
        private string GetCompetitionDisplayName(Fixture fixture)
        {
            if (fixture == null) return "Unknown competition";

            // 1) League template name (най-добър вариант за league)
            var leagueTemplateName = fixture.League?.Template?.Name;
            if (!string.IsNullOrWhiteSpace(leagueTemplateName))
            {
                // добавяме ниво (tier) ако има и ако е полезно
                if (fixture.League?.Tier > 0)
                    return $"{leagueTemplateName} (Tier {fixture.League.Tier})";
                return leagueTemplateName;
            }

            // 2) Cup template / round (DomesticCup)
            var cupTemplateName = fixture.CupRound?.Cup?.Template?.Name;
            if (!string.IsNullOrWhiteSpace(cupTemplateName))
            {
                var roundName = fixture.CupRound?.Name;
                return string.IsNullOrWhiteSpace(roundName)
                    ? cupTemplateName
                    : $"{cupTemplateName} — {roundName}";
            }

            // 3) European cup: try phase's cup template, or phase template name
            var euroCupTemplateName = fixture.EuropeanCupPhase?.EuropeanCup?.Template?.Name;
            if (!string.IsNullOrWhiteSpace(euroCupTemplateName))
            {
                var phaseName = fixture.EuropeanCupPhase?.PhaseTemplate?.Name;
                // phase template name може да е нещо като "Group Stage" или "Quarterfinals"
                return string.IsNullOrWhiteSpace(phaseName)
                    ? euroCupTemplateName
                    : $"{euroCupTemplateName} — {phaseName}";
            }

            // (if you have Competition navigation filled on League/Cup/EuropeanCup)
            var compFromLeague = fixture.League?.Competition;
            if (compFromLeague != null)
            {

            }

            // 5) Fallbacks: use CupRound.Name if present, or EuropeanCupPhase info, or enum name
            if (!string.IsNullOrWhiteSpace(fixture.CupRound?.Name))
                return fixture.CupRound.Name;

            var phaseTemplateName = fixture.EuropeanCupPhase?.PhaseTemplate?.Name;
            if (!string.IsNullOrWhiteSpace(phaseTemplateName))
                return phaseTemplateName;

            // 6) Final fallback: use CompetitionTypeEnum with maybe league tier or round
            var baseName = fixture.CompetitionType.ToString();
            if (fixture.CompetitionType == CompetitionTypeEnum.League && fixture.League != null)
                return $"{baseName} (Tier {fixture.League.Tier})";

            if (fixture.CompetitionType == CompetitionTypeEnum.DomesticCup && fixture.CupRound != null)
                return $"{baseName} — {fixture.CupRound?.Name ?? "Cup"}";

            return baseName;
        }
    }
}
