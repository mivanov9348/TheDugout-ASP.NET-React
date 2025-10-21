namespace TheDugout.Services.Game
{
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Season.Interfaces;
    using TheDugout.Services.Staff;
    using TheDugout.Services.Team.Interfaces;

    public class GameSaveService : IGameSaveService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameSaveService> _logger;
        private readonly ILeagueService _leagueGenerator;
        private readonly INewSeasonService _seasonGenerator;
        private readonly ILeagueFixturesService _leagueFixturesService;
        private readonly IEurocupFixturesService _eurocupFixturesService;
        private readonly IPlayerGenerationService _playerGenerator;
        private readonly IBankService _bankService;
        private readonly ITeamFinanceService _teamFinanceService;
        private readonly ITeamGenerationService _teamGenerator;
        private readonly ITeamPlanService _teamPlanService;
        private readonly IEuropeanCupService _europeanCupService;
        private readonly ICupService _cupService;
        private readonly IAgencyService _agencyService;
        private readonly IGameSettingsService _gameSettings;

        public GameSaveService(
            DugoutDbContext context,
            ILogger<GameSaveService> logger,
            ILeagueService leagueGenerator,
            INewSeasonService seasonGenerator,
            IPlayerGenerationService playerGenerator,
            IBankService bankService,
            ITeamFinanceService teamFinanceService,
            ITeamPlanService teamPlanService,
            IEuropeanCupService europeanCupService,
            ITeamGenerationService teamGenerator,
            ICupService cupService,
            IAgencyService agencyService,
            ILeagueFixturesService leagueFixturesService,
            IEurocupFixturesService eurocupFixturesService,
            IGameSettingsService gameSettings
        )
        {
            _context = context;
            _logger = logger;
            _leagueGenerator = leagueGenerator;
            _seasonGenerator = seasonGenerator;
            _playerGenerator = playerGenerator;
            _bankService = bankService;
            _teamFinanceService = teamFinanceService;
            _teamPlanService = teamPlanService;
            _europeanCupService = europeanCupService;
            _teamGenerator = teamGenerator;
            _cupService = cupService;
            _agencyService = agencyService;
            _leagueFixturesService = leagueFixturesService;
            _eurocupFixturesService = eurocupFixturesService;
            _gameSettings = gameSettings;
        }

        public async Task<List<object>> GetUserSavesAsync(int userId)
        {
            return await _context.GameSaves
                .AsNoTracking()
                .Where(gs => gs.UserId == userId)
                .OrderByDescending(gs => gs.CreatedAt)
                .Select(gs => new { gs.Id, gs.Name, gs.CreatedAt })
                .ToListAsync<object>();
        }

        public async Task<GameSave?> GetGameSaveAsync(int userId, int saveId)
        {
            return await _context.GameSaves
                .AsSplitQuery()
                .Include(gs => gs.Leagues)
                    .ThenInclude(l => l.Teams)
                    .ThenInclude(t => t.Players)
                .Include(gs => gs.Seasons)
                    .ThenInclude(s => s.Events)
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId);
        }
        public async Task<bool> DeleteGameSaveAsync(int saveId)
        {
            // Увеличаваме timeout-а, защото каскадните изтривания могат да са бавни
            _context.Database.SetCommandTimeout(180);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"=== HARD DELETE START for GameSave #{saveId} ===");

                // Проверка дали сейвът съществува
                var exists = await _context.GameSaves.AnyAsync(gs => gs.Id == saveId);
                if (!exists)
                {
                    Console.WriteLine($"❌ GameSave with Id={saveId} not found.");
                    return false;
                }

                // Определяне на SQL кавичките според провайдъра
                var provider = _context.Database.ProviderName?.ToLower() ?? "";
                string Quote(string identifier) =>
                    provider.Contains("sqlserver") ? $"[{identifier}]"
                    : provider.Contains("sqlite") ? $"\"{identifier}\""
                    : provider.Contains("mysql") ? $"`{identifier}`"
                    : provider.Contains("npgsql") ? $"\"{identifier}\""
                    : identifier;

                // 1️⃣ Изключваме всички foreign key constraint-и (временно)
                Console.WriteLine("⚙️ Disabling foreign key constraints...");
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'");

                // 2️⃣ Нулираме връзките от други таблици към сейфа
                Console.WriteLine("Clearing foreign key references...");

                var usersSql = $"UPDATE {Quote("Users")} SET {Quote("CurrentSaveId")} = NULL WHERE {Quote("CurrentSaveId")} = @p0;";
                await _context.Database.ExecuteSqlRawAsync(usersSql, saveId);

                var clearUserTeamSql = $"UPDATE {Quote("GameSaves")} SET {Quote("UserTeamId")} = NULL WHERE {Quote("Id")} = @p0;";
                await _context.Database.ExecuteSqlRawAsync(clearUserTeamSql, saveId);

                // 3️⃣ Таблици, които се триeт чрез JOIN (нямат GameSaveId)
                Console.WriteLine("Deleting dependent tables via JOIN...");

                var joinDeletes = new[]
                {
            ("TeamTactics", "TeamId"),
            ("TrainingFacilities", "TeamId"),
            ("YouthAcademies", "TeamId")
        };

                foreach (var (table, fk) in joinDeletes)
                {
                    var joinSql = $@"
DELETE {Quote(table)}
FROM {Quote(table)} 
INNER JOIN {Quote("Teams")} T ON {Quote(table)}.{Quote(fk)} = T.{Quote("Id")}
WHERE T.{Quote("GameSaveId")} = @p0;";
                    Console.WriteLine($"Deleting from {table} (via TeamId)...");
                    await _context.Database.ExecuteSqlRawAsync(joinSql, saveId);
                }

                // 4️⃣ Таблици с GameSaveId — по правилния ред
                var tables = new[]
                {
            // 🔺 Родителските таблици първо
            "Seasons",
            "Leagues",
            "Competitions",
            "Teams",
            "Stadiums",

            // 🔹 Всички останали
            "PlayerAttributes",
            "MatchEvents",
            "Penalties",
                        "PlayerCompetitionStats",
            "PlayerMatchStats",
            "PlayerSeasonStats",
            "PlayerTrainings",
             "TransferOffers",
            "Transfers",
            "SeasonEvents",
            "Messages",
            "Players",
            "TrainingSessions",
            "FinancialTransactions",
            "Matches",
            "Fixtures",
            "CupRounds",
            "EuropeanCupPhases",
            "EuropeanCupStandings",
            "EuropeanCupTeams",
            "CupTeams",
            "LeagueStandings",
            "EuropeanCups",
            "Cups",
            "Agencies",
            "Banks",
            "CompetitionEuropeanQualifiedTeams",
            "CompetitionPromotedTeams",
            "CompetitionRelegatedTeams",
            "CompetitionSeasonResults",
            "CompetitionAwards",
        };

                foreach (var table in tables)
                {
                    Console.WriteLine($"Deleting from {table}...");
                    var sql = $"DELETE FROM {Quote(table)} WHERE {Quote("GameSaveId")} = @p0;";
                    await _context.Database.ExecuteSqlRawAsync(sql, saveId);
                }

                // 5️⃣ Изтриваме самия сейв
                Console.WriteLine("Deleting GameSave record...");
                var deleteSaveSql = $"DELETE FROM {Quote("GameSaves")} WHERE {Quote("Id")} = @p0;";
                await _context.Database.ExecuteSqlRawAsync(deleteSaveSql, saveId);

                // 6️⃣ Връщаме constraint-ите обратно
                Console.WriteLine("✅ Re-enabling foreign key constraints...");
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");

                // 7️⃣ Потвърждаваме транзакцията
                await transaction.CommitAsync();

                Console.WriteLine($"✅ GameSave #{saveId} deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ ERROR deleting GameSave {saveId}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                // Връщаме constraint-ите, дори при грешка
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");
                }
                catch { }

                return false;
            }
        }

        public async Task<GameSave> StartNewGameAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid userId.");

            var saveCount = await _context.GameSaves.CountAsync(gs => gs.UserId == userId);
            if (saveCount >= 3)
                throw new InvalidOperationException("3 saves max!");

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                void LogStep(string step)
                {
                    stopwatch.Stop();
                    Console.WriteLine($"⏱ {step} completed in {stopwatch.ElapsedMilliseconds} ms");
                    stopwatch.Restart();
                }

                // 1️⃣ Създаваме нов save
                var gameSave = new GameSave
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                };

                _context.GameSaves.Add(gameSave);
                await _context.SaveChangesAsync(ct);
                LogStep("Created GameSave");

                // 2️⃣ Банка / финанси
                var initialBalance = await _gameSettings.GetIntAsync("bankInitial") ?? 200000000;
                await _bankService.CreateBankAsync(gameSave, initialBalance);
                await _context.SaveChangesAsync(ct);
                LogStep("Created Bank and Finance data");

                // 3️⃣ Агенции
                await _agencyService.InitializeAgenciesForGameSaveAsync(gameSave);
                LogStep("Initialized Agencies");

                // 4️⃣ Сезон
                var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
                var season = await _seasonGenerator.GenerateSeason(gameSave, startDate);
                gameSave.Seasons.Add(season);
                await _context.SaveChangesAsync(ct);
                LogStep("Generated Season");

                // 5️⃣ Лиги + отбори
                var leagues = await _leagueGenerator.GenerateLeaguesAsync(gameSave, season);
                await _context.SaveChangesAsync(ct);
                LogStep("Generated Leagues");

                var independentTeams = await _teamGenerator.GenerateIndependentTeamsAsync(gameSave);
                foreach (var team in independentTeams)
                    gameSave.Teams.Add(team);

                await _context.SaveChangesAsync(ct);
                LogStep("Generated Independent Teams");

                await _teamFinanceService.InitializeClubFundsAsync(gameSave, leagues);
                LogStep("Initialized Club Funds");

                // 6️⃣ European Cups (ако има шаблони)
                var euroTemplates = await _context.Set<EuropeanCupTemplate>()
                    .Include(t => t.PhaseTemplates)
                    .Where(t => t.IsActive)
                    .ToListAsync(ct);
                LogStep("Loaded European Cup Templates");

                foreach (var template in euroTemplates)
                {
                    try
                    {
                        var eligibleTeams = await _context.Set<Models.Teams.Team>()
                            .Where(t => t.LeagueId == null && t.GameSaveId == gameSave.Id)
                            .ToListAsync(ct);

                        if (eligibleTeams.Count < template.TeamsCount)
                        {
                            _logger.LogWarning(
                                "Not enough eligible teams ({Eligible}) for European Cup Template '{TemplateName}' ({TemplateId}). Requires {Required}. Skipping.",
                                eligibleTeams.Count, template.Name, template.Id, template.TeamsCount);
                            continue;
                        }

                        await _europeanCupService.InitializeTournamentAsync(
                            templateId: template.Id,
                            gameSaveId: gameSave.Id,
                            seasonId: season.Id,
                            ct: ct);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to initialize European Cup template '{TemplateName}' ({TemplateId})",
                            template.Name, template.Id);
                    }
                }
                LogStep("Initialized European Cups");

                // 7️⃣ Cups
                await _cupService.InitializeCupsForGameSaveAsync(gameSave, season.Id);
                LogStep("Initialized Domestic Cups");

                // 8️⃣ Fixtures
                await _leagueFixturesService.GenerateLeagueFixturesAsync(gameSave.Id, season.Id, startDate);
                LogStep("Generated League Fixtures");

                // 9️⃣ Standings
                await _context.SaveChangesAsync(ct);
                await _leagueGenerator.InitializeStandingsAsync(gameSave, season);
                LogStep("Initialized League Standings");

                // 🔟 Дефолтни тактики
                await _teamPlanService.InitializeDefaultTacticsAsync(gameSave);
                LogStep("Initialized Default Tactics");

                // ✅ Commit
                await transaction.CommitAsync(ct);
                LogStep("Committed Transaction");

                // 🔁 Зареждаме резултата
                var result = await _context.GameSaves
                            .AsSplitQuery()
                            .Include(gs => gs.Leagues)
                                .ThenInclude(l => l.Teams)
                                .ThenInclude(t => t.Players)
                            .Include(gs => gs.Seasons)
                                .ThenInclude(s => s.Events)
                            .FirstAsync(gs => gs.Id == gameSave.Id, ct);
                LogStep("Loaded Final GameSave");

                Console.WriteLine($"🏁 Total time: {stopwatch.ElapsedMilliseconds} ms");
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                stopwatch.Stop();
                Console.WriteLine($"❌ ERROR: Transaction rolled back after {stopwatch.ElapsedMilliseconds} ms");
                throw;
            }
        }
    }
}
