using Microsoft.EntityFrameworkCore;
using System.Linq;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Game;
using TheDugout.Services.Cup;
using TheDugout.Services.EuropeanCup;
using TheDugout.Services.Finance;
using TheDugout.Services.League;
using TheDugout.Services.Players;
using TheDugout.Services.Season;
using TheDugout.Services.Staff;
using TheDugout.Services.Team;

namespace TheDugout.Services.Game
{
    public class GameSaveService : IGameSaveService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameSaveService> _logger;
        private readonly ILeagueService _leagueGenerator;
        private readonly ISeasonGenerationService _seasonGenerator;
        private readonly ILeagueFixturesService _leagueFixturesService;
        private readonly IEurocupFixturesService _eurocupFixturesService;
        private readonly IPlayerGenerationService _playerGenerator;
        private readonly IFinanceService _financeService;
        private readonly ITeamGenerationService _teamGenerator;
        private readonly ITeamPlanService _teamPlanService;
        private readonly IEuropeanCupService _europeanCupService;
        private readonly ICupService _cupService;
        private readonly IAgencyService _agencyService;

        public GameSaveService(
            DugoutDbContext context,
            ILogger<GameSaveService> logger,
            ILeagueService leagueGenerator,
            ISeasonGenerationService seasonGenerator,
            IPlayerGenerationService playerGenerator,
            IFinanceService financeService,
            ITeamPlanService teamPlanService,
            IEuropeanCupService europeanCupService,
            ITeamGenerationService teamGenerator,
            ICupService cupService,
            IAgencyService agencyService,
            ILeagueFixturesService leagueFixturesService,
            IEurocupFixturesService eurocupFixturesService
        )
        {
            _context = context;
            _logger = logger;
            _leagueGenerator = leagueGenerator;
            _seasonGenerator = seasonGenerator;
            _playerGenerator = playerGenerator;
            _financeService = financeService;
            _teamPlanService = teamPlanService;
            _europeanCupService = europeanCupService;
            _teamGenerator = teamGenerator;
            _cupService = cupService;
            _agencyService = agencyService;
            _leagueFixturesService = leagueFixturesService;
            _eurocupFixturesService = eurocupFixturesService;
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"=== START DELETE PROCESS for GameSave #{saveId} ===");

                var gameSave = await _context.GameSaves.FirstOrDefaultAsync(gs => gs.Id == saveId);
                if (gameSave == null)
                {
                    Console.WriteLine($"❌ GameSave with Id={saveId} not found.");
                    return false;
                }

                // === STEP 1: Извличане на ID-та ===
                var playerIds = await _context.Players
                    .Where(p => p.GameSaveId == saveId)
                    .Select(p => p.Id)
                    .ToListAsync();

                var teamIds = await _context.Teams
                    .Where(t => t.GameSaveId == saveId)
                    .Select(t => t.Id)
                    .ToListAsync();

                var matchIds = await _context.Matches
                    .Where(m => m.GameSaveId == saveId)
                    .Select(m => m.Id)
                    .ToListAsync();

                var cupIds = await _context.Cups
                    .Where(c => c.GameSaveId == saveId)
                    .Select(c => c.Id)
                    .ToListAsync();

                Console.WriteLine($"Loaded: {playerIds.Count} players, {teamIds.Count} teams, {matchIds.Count} matches, {cupIds.Count} cups");

                // === STEP 2: Изтриване на зависими таблици ===
                Console.WriteLine("Deleting nested entities...");

                await _context.PlayerAttributes.Where(a => playerIds.Contains(a.PlayerId ?? -1)).ExecuteDeleteAsync();
                await _context.PlayerMatchStats.Where(s => playerIds.Contains(s.PlayerId)).ExecuteDeleteAsync();
                await _context.PlayerSeasonStats.Where(s => playerIds.Contains(s.PlayerId ?? -1)).ExecuteDeleteAsync();
                await _context.PlayerTrainings.Where(s => playerIds.Contains(s.PlayerId ?? -1)).ExecuteDeleteAsync();

                await _context.MatchEvents.Where(e => matchIds.Contains(e.MatchId)).ExecuteDeleteAsync();
                await _context.Penalties.Where(p => matchIds.Contains(p.MatchId ?? -1)).ExecuteDeleteAsync();

                await _context.Stadiums.Where(s => teamIds.Contains(s.TeamId ?? -1)).ExecuteDeleteAsync();
                await _context.TeamTactics.Where(t => teamIds.Contains(t.TeamId ?? -1)).ExecuteDeleteAsync();
                await _context.TrainingFacilities.Where(f => teamIds.Contains(f.TeamId ?? -1)).ExecuteDeleteAsync();

                // === STEP 3: Изтриване на основни таблици ===
                Console.WriteLine("Deleting main linked entities...");

                await _context.Matches.Where(m => matchIds.Contains(m.Id)).ExecuteDeleteAsync();
                await _context.Players.Where(p => playerIds.Contains(p.Id)).ExecuteDeleteAsync();
                await _context.Teams.Where(t => teamIds.Contains(t.Id)).ExecuteDeleteAsync();
                await _context.Cups.Where(c => cupIds.Contains(c.Id)).ExecuteDeleteAsync();

                await _context.Leagues.Where(l => l.GameSaveId == saveId).ExecuteDeleteAsync();
                await _context.Seasons.Where(s => s.GameSaveId == saveId).ExecuteDeleteAsync();
                await _context.TrainingSessions.Where(ts => ts.GameSaveId == saveId).ExecuteDeleteAsync();
                await _context.LeagueStandings.Where(ls => ls.GameSaveId == saveId).ExecuteDeleteAsync();
                await _context.Agencies.Where(a => a.GameSaveId == saveId).ExecuteDeleteAsync();
                await _context.Messages.Where(m => m.GameSaveId == saveId).ExecuteDeleteAsync();
                await _context.Fixtures.Where(f => f.GameSaveId == saveId).ExecuteDeleteAsync();

                // === STEP 4: Изтриване на самия GameSave ===
                Console.WriteLine($"Deleting GameSave #{saveId}");
                await _context.GameSaves.Where(gs => gs.Id == saveId).ExecuteDeleteAsync();

                await transaction.CommitAsync();
                Console.WriteLine("✅ GameSave deleted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ ERROR deleting GameSave {saveId}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
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

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Създаваме нов save
                var gameSave = new GameSave
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Name = $"Save_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                };

                _context.GameSaves.Add(gameSave);
                await _context.SaveChangesAsync();

                // 2. Банка / финанси
                await _financeService.CreateBankAsync(gameSave);
                await _context.SaveChangesAsync();

                // Инициализация на агенции
                await _agencyService.InitializeAgenciesForGameSaveAsync(gameSave);

                // 3. Създаваме първи сезон
                var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
                var season = await _seasonGenerator.GenerateSeason(gameSave, startDate);
                gameSave.Seasons.Add(season);
                await _context.SaveChangesAsync();

                // 4. Генерираме лиги + отбори
                var leagues = await _leagueGenerator.GenerateLeaguesAsync(gameSave, season);
                await _context.SaveChangesAsync();

                var independentTeams = await _teamGenerator.GenerateIndependentTeamsAsync(gameSave);
                foreach (var team in independentTeams)
                {
                    gameSave.Teams.Add(team);
                }

                await _context.SaveChangesAsync();

                await _financeService.InitializeClubFundsAsync(gameSave, leagues);

                // 4.5 Инициализиране на European Cup за първата година (ако имаш шаблон)
                var euroTemplates = await _context.Set<EuropeanCupTemplate>()
                                    .Include(t => t.PhaseTemplates)
                                    .Where(t => t.IsActive)
                                    .ToListAsync(ct);

                foreach (var template in euroTemplates)
                {
                    try
                    {
                        // 1. Вземаме подходящи отбори: LeagueId == null и същия GameSave
                        var eligibleTeams = await _context.Set<Models.Teams.Team>()
                            .Where(t => t.LeagueId == null && t.GameSaveId == gameSave.Id)
                            .ToListAsync(ct);

                        if (eligibleTeams.Count < template.TeamsCount)
                        {
                            _logger.LogWarning(
                                "Not enough eligible teams ({Eligible}) for European Cup Template '{TemplateName}' ({TemplateId}). Requires {Required}. Skipping.",
                                eligibleTeams.Count, template.Name, template.Id, template.TeamsCount);
                            continue; // прескачаме, но не прекъсваме цялото стартиране
                        }

                        // 2. Създаваме турнир
                        var cup = await _europeanCupService.InitializeTournamentAsync(
                            templateId: template.Id,
                            gameSaveId: gameSave.Id,
                            seasonId: season.Id,
                            ct: ct);

                        _logger.LogInformation(
                            "Successfully initialized European Cup '{TemplateName}' (ID: {CupId}) with {Teams} teams.",
                            template.Name, cup.Id, template.TeamsCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to initialize European Cup template '{TemplateName}' ({TemplateId})",
                            template.Name, template.Id);
                    }
                }

                // Generating Cup
                await _cupService.InitializeCupsForGameSaveAsync(gameSave, season.Id);

                // 6. Генерираме league fixtures
                await _leagueFixturesService.GenerateLeagueFixturesAsync(gameSave.Id, season.Id, startDate);

                // 7. Инициализираме standings (таблици за класиране)
                await _context.SaveChangesAsync();
                await _leagueGenerator.InitializeStandingsAsync(gameSave, season);

                // 8. Дефолтни тактики
                await _teamPlanService.InitializeDefaultTacticsAsync(gameSave);

                // Commit
                await transaction.CommitAsync();

                // 9. Връщаме пълния save с данни
                return await _context.GameSaves
                    .Include(gs => gs.Leagues).ThenInclude(l => l.Teams).ThenInclude(t => t.Players)
                    .Include(gs => gs.Seasons).ThenInclude(s => s.Events)
                    .FirstAsync(gs => gs.Id == gameSave.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


    }
}
