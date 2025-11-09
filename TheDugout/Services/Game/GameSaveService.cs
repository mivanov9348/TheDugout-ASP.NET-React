namespace TheDugout.Services.Game
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.Game.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Season.Interfaces;
    using TheDugout.Services.Staff.Interfaces;
    using TheDugout.Services.Team.Interfaces;

    public class GameSaveService : IGameSaveService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameSaveService> _logger;
        private readonly ILeagueService _leagueGenerator;
        private readonly INewSeasonService _seasonGenerator;
        private readonly ILeagueFixturesService _leagueFixturesService;
        private readonly IBankService _bankService;
        private readonly ITeamFinanceService _teamFinanceService;
        private readonly ITeamGenerationService _teamGenerator;
        private readonly IEuropeanCupService _europeanCupService;
        private readonly ICupService _cupService;
        private readonly IAgencyService _agencyService;
        private readonly IGameSettingsService _gameSettings;
        private readonly IYouthPlayerService _youthPlayerService;

        public GameSaveService(
            DugoutDbContext context,
            ILogger<GameSaveService> logger,
            ILeagueService leagueGenerator,
            INewSeasonService seasonGenerator,
            IBankService bankService,
            ITeamFinanceService teamFinanceService,
            IEuropeanCupService europeanCupService,
            ITeamGenerationService teamGenerator,
            ICupService cupService,
            IAgencyService agencyService,
            ILeagueFixturesService leagueFixturesService,
            IGameSettingsService gameSettings,
            IYouthPlayerService youthPlayerService
        )
        {
            _context = context;
            _logger = logger;
            _leagueGenerator = leagueGenerator;
            _seasonGenerator = seasonGenerator;
            _bankService = bankService;
            _teamFinanceService = teamFinanceService;
            _europeanCupService = europeanCupService;
            _teamGenerator = teamGenerator;
            _cupService = cupService;
            _agencyService = agencyService;
            _leagueFixturesService = leagueFixturesService;
            _gameSettings = gameSettings;
            _youthPlayerService = youthPlayerService;
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
                        "YouthPlayers",

            "PlayerAttributes",
            "MatchEvents",
            "Penalties",
           "PlayerCompetitionStats",
            "PlayerMatchStats",
            "PlayerSeasonStats",
            "PlayerTrainings",
            "TeamTrainingPlans",

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

                try
                {
                    await _context.Database.ExecuteSqlRawAsync("EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");
                }
                catch { }

                return false;
            }
        }

        public async IAsyncEnumerable<string> StartNewGameStreamAsync(
    int? userId,
    [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid userId.");

            var saveCount = await _context.GameSaves.CountAsync(gs => gs.UserId == userId, ct);
            if (saveCount >= 3)
                throw new InvalidOperationException("3 saves max!");

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            yield return "▶️ Creating new game save...";
            var gameSave = new GameSave
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
            };
            _context.GameSaves.Add(gameSave);
            await _context.SaveChangesAsync(ct);

            yield return "💰 Initializing bank and finances...";
            var initialBalance = await _gameSettings.GetIntAsync("bankInitial") ?? 200000000;
            await _bankService.CreateBankAsync(gameSave, initialBalance);
            await _context.SaveChangesAsync(ct);

            yield return "🕴 Initializing agencies...";
            await _agencyService.InitializeAgenciesForGameSaveAsync(gameSave);

            yield return "📆 Generating season...";
            var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
            var season = await _seasonGenerator.GenerateSeason(gameSave, startDate);
            gameSave.Seasons.Add(season);
            await _context.SaveChangesAsync(ct);

            yield return "🏆 Generating leagues...";
            var leagues = await _leagueGenerator.GenerateLeaguesAsync(gameSave, season);
            await _context.SaveChangesAsync(ct);

            yield return "⚽ Generating league teams...";
            await _leagueGenerator.GenerateTeamsForLeaguesAsync(gameSave, leagues);
            await _context.SaveChangesAsync(ct);

            yield return "🏫 Generating independent teams...";
            var independentTeams = await _teamGenerator.GenerateIndependentTeamsAsync(gameSave);
            foreach (var team in independentTeams)
                gameSave.Teams.Add(team);
            await _context.SaveChangesAsync(ct);

            yield return "💸 Initializing club funds...";
            await _teamFinanceService.InitializeClubFundsAsync(gameSave, leagues);

            yield return "👶 Generating youth intakes...";
            var academies = await _context.YouthAcademies
                .Include(a => a.Team)
                    .ThenInclude(t => t.Country)
                .Where(a => a.Team.GameSaveId == gameSave.Id)
                .ToListAsync(ct);

            foreach (var academy in academies)
            {
                try
                {
                    await _youthPlayerService.GenerateAllYouthIntakesAsync(academy, gameSave, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to generate youth intake for {Team}", academy.Team?.Name);
                }
            }

            yield return "🌍 Loading European Cup templates...";
            var euroTemplates = await _context.Set<EuropeanCupTemplate>()
                .Include(t => t.PhaseTemplates)
                .Where(t => t.IsActive)
                .ToListAsync(ct);

            yield return "🏅 Initializing European Cups...";
            foreach (var template in euroTemplates)
            {
                try
                {
                    await _europeanCupService.InitializeTournamentAsync(
                        template.Id, gameSave.Id, season.Id, season.Id, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to init European Cup {Name}", template.Name);
                }
            }

            yield return "🥇 Initializing domestic cups...";
            await _cupService.InitializeCupsForGameSaveAsync(gameSave, season.Id);

            yield return "📅 Generating league fixtures...";
            await _leagueFixturesService.GenerateLeagueFixturesAsync(gameSave.Id, season.Id, startDate);

            yield return "📊 Initializing standings...";
            await _context.SaveChangesAsync(ct);
            await _leagueGenerator.InitializeStandingsAsync(gameSave, season);

            yield return "✅ Committing transaction...";
            await transaction.CommitAsync(ct);

            yield return "👤 Resetting current save...";
            var user = await _context.Users.FirstAsync(u => u.Id == userId, ct);
            user.CurrentSaveId = null;
            await _context.SaveChangesAsync(ct);

            yield return $"RESULT|{gameSave.Id}";
            yield return "🏁 Game save ready!";
        }
    }
}
