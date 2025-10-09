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
            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(gs => gs.Id == saveId);

            if (gameSave == null)
                throw new ArgumentException($"❌ GameSave with id {saveId} not found.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine($"🧹 Starting cascade delete for GameSave {saveId}");

                // =============== 1️⃣ ID извличане ===============
                var teams = await _context.Teams.Where(t => t.GameSaveId == saveId).Select(t => t.Id).ToListAsync();
                var players = await _context.Players.Where(p => p.GameSaveId == saveId).Select(p => p.Id).ToListAsync();
                var seasons = await _context.Seasons.Where(s => s.GameSaveId == saveId).Select(s => s.Id).ToListAsync();
                var leagues = await _context.Leagues.Where(l => l.GameSaveId == saveId).Select(l => l.Id).ToListAsync();
                var cups = await _context.Cups.Where(c => c.GameSaveId == saveId).Select(c => c.Id).ToListAsync();
                var europeanCups = await _context.EuropeanCups.Where(ec => ec.GameSaveId == saveId).Select(ec => ec.Id).ToListAsync();
                var fixtures = await _context.Fixtures.Where(f => f.GameSaveId == saveId).Select(f => f.Id).ToListAsync();
                var leagueFixtures = await _context.Fixtures.Where(f => leagues.Contains(f.League.Id)).Select(f => f.Id).ToListAsync();
                var euroPhases = await _context.EuropeanCupPhases
                    .Where(p => europeanCups.Contains(p.EuropeanCupId))
                    .Select(p => p.Id)
                    .ToListAsync();

                // =============== 2️⃣ Актуализиране на nullable външни ключове ===============
                // Актуализиране на GameSaves.UserTeamId
                await SafeDeleteAsync(async () =>
                {
                    var gameSaves = await _context.GameSaves
                        .Where(gs => gs.Id == saveId)
                        .ToListAsync();
                    foreach (var gs in gameSaves)
                    {
                        gs.UserTeamId = null;
                    }
                    await _context.SaveChangesAsync();
                    return gameSaves.Count;
                }, "GameSaves (UserTeamId cleanup)");

                // Актуализиране на Fixtures (HomeTeamId, AwayTeamId, WinnerTeamId)
                await SafeDeleteAsync(async () =>
                {
                    var updatedFixtures = await _context.Fixtures
                        .Where(f => f.GameSaveId == saveId)
                        .ToListAsync();
                    foreach (var f in updatedFixtures)
                    {
                        f.HomeTeamId = null;
                        f.AwayTeamId = null;
                        f.WinnerTeamId = null;
                    }
                    await _context.SaveChangesAsync();
                    return updatedFixtures.Count;
                }, "Fixtures (Team IDs cleanup)");

                // Актуализиране на Transfers (FromTeamId, ToTeamId)
                await SafeDeleteAsync(async () =>
                {
                    var updatedTransfers = await _context.Transfers
                        .Where(tr => teams.Contains(tr.FromTeam.Id) || teams.Contains(tr.ToTeam.Id))
                        .ToListAsync();
                    foreach (var tr in updatedTransfers)
                    {
                        tr.FromTeamId = null;
                        tr.ToTeamId = null;
                    }
                    await _context.SaveChangesAsync();
                    return updatedTransfers.Count;
                }, "Transfers (Team IDs cleanup)");

                // Актуализиране на Players.TeamId
                await SafeDeleteAsync(async () =>
                {
                    var updatedPlayers = await _context.Players
                        .Where(p => p.GameSaveId == saveId)
                        .ToListAsync();
                    foreach (var p in updatedPlayers)
                    {
                        p.TeamId = null;
                    }
                    await _context.SaveChangesAsync();
                    return updatedPlayers.Count;
                }, "Players (TeamId cleanup)");

                // =============== 3️⃣ Дълбоки зависимости ===============
                await SafeDeleteAsync(() => _context.PlayerAttributes
                    .Where(pa => players.Contains(pa.PlayerId)).ExecuteDeleteAsync(), "PlayerAttributes");
                await SafeDeleteAsync(() => _context.PlayerTrainings
                    .Where(pt => players.Contains(pt.PlayerId)).ExecuteDeleteAsync(), "PlayerTrainings");
                await SafeDeleteAsync(() => _context.PlayerMatchStats
                    .Where(pms => players.Contains(pms.PlayerId)).ExecuteDeleteAsync(), "PlayerMatchStats");
                await SafeDeleteAsync(() => _context.PlayerSeasonStats
                    .Where(pss => players.Contains(pss.PlayerId)).ExecuteDeleteAsync(), "PlayerSeasonStats");
                await SafeDeleteAsync(() => _context.TrainingFacilities
                    .Where(tf => teams.Contains(tf.TeamId)).ExecuteDeleteAsync(), "TrainingFacilities");
                await SafeDeleteAsync(() => _context.YouthAcademies
                    .Where(ya => teams.Contains(ya.TeamId)).ExecuteDeleteAsync(), "YouthAcademies");
                await SafeDeleteAsync(() => _context.Stadiums
                    .Where(s => teams.Contains(s.TeamId)).ExecuteDeleteAsync(), "Stadiums");
                await SafeDeleteAsync(() => _context.TeamTactics
                    .Where(tt => teams.Contains(tt.TeamId)).ExecuteDeleteAsync(), "TeamTactics");
                await SafeDeleteAsync(() => _context.MatchEvents
                    .Where(me => leagueFixtures.Contains(me.Match.FixtureId) || teams.Contains(me.TeamId))
                    .ExecuteDeleteAsync(), "MatchEvents");
                await SafeDeleteAsync(() => _context.Penalties
                    .Where(p => leagueFixtures.Contains(p.Match.FixtureId) || teams.Contains(p.TeamId))
                    .ExecuteDeleteAsync(), "Penalties");
                await SafeDeleteAsync(() => _context.Matches
                    .Where(m => m.GameSaveId == saveId).ExecuteDeleteAsync(), "Matches");
                await SafeDeleteAsync(() => _context.FinancialTransactions
                    .Where(ft => teams.Contains(ft.FromTeam.Id) || teams.Contains(ft.ToTeam.Id))
                    .ExecuteDeleteAsync(), "FinancialTransactions");
                await SafeDeleteAsync(() => _context.Transfers
                    .Where(tr => players.Contains(tr.PlayerId)).ExecuteDeleteAsync(), "Transfers");
                await SafeDeleteAsync(() => _context.Fixtures
                    .Where(f => cups.Contains(f.CupRound.CupId)).ExecuteDeleteAsync(), "Fixtures (Cup)");
                await SafeDeleteAsync(() => _context.CupTeams
                    .Where(ct => cups.Contains(ct.CupId)).ExecuteDeleteAsync(), "CupTeams");
                await SafeDeleteAsync(() => _context.CupRounds
                    .Where(cr => cups.Contains(cr.CupId)).ExecuteDeleteAsync(), "CupRounds");
                await SafeDeleteAsync(() => _context.Fixtures
                    .Where(f => euroPhases.Contains(f.EuropeanCupPhase.Id)).ExecuteDeleteAsync(), "Fixtures (EuroPhase)");
                await SafeDeleteAsync(() => _context.EuropeanCupTeams
                    .Where(et => europeanCups.Contains(et.EuropeanCupId)).ExecuteDeleteAsync(), "EuropeanCupTeams");
                await SafeDeleteAsync(() => _context.EuropeanCupStandings
                    .Where(es => europeanCups.Contains(es.EuropeanCupId)).ExecuteDeleteAsync(), "EuropeanCupStandings");
                await SafeDeleteAsync(() => _context.EuropeanCupPhases
                    .Where(p => europeanCups.Contains(p.EuropeanCupId)).ExecuteDeleteAsync(), "EuropeanCupPhases");
                await SafeDeleteAsync(() => _context.TrainingSessions
                    .Where(ts => teams.Contains(ts.TeamId)).ExecuteDeleteAsync(), "TrainingSessions");
                await SafeDeleteAsync(() => _context.SeasonEvents
                    .Where(se => seasons.Contains(se.SeasonId)).ExecuteDeleteAsync(), "SeasonEvents");

                // =============== 4️⃣ Средни зависимости ===============

                await SafeDeleteAsync(() => _context.LeagueStandings
                    .Where(ls => leagues.Contains(ls.LeagueId)).ExecuteDeleteAsync(), "LeagueStandings");

                await SafeDeleteAsync(() => _context.Teams
                    .Where(t => t.GameSaveId == saveId).ExecuteDeleteAsync(), "Teams");

                await SafeDeleteAsync(() => _context.Messages
                    .Where(m => m.GameSaveId == saveId).ExecuteDeleteAsync(), "Messages");

                await SafeDeleteAsync(() => _context.Fixtures
                    .Where(f => leagues.Contains(f.League.Id))
                    .ExecuteDeleteAsync(), "Fixtures (League)");

                await SafeDeleteAsync(() => _context.Leagues
                    .Where(l => l.GameSaveId == saveId).ExecuteDeleteAsync(), "Leagues");

                await SafeDeleteAsync(() => _context.Cups
                    .Where(c => c.GameSaveId == saveId).ExecuteDeleteAsync(), "Cups");

                await SafeDeleteAsync(() => _context.EuropeanCups
                    .Where(ec => ec.GameSaveId == saveId).ExecuteDeleteAsync(), "EuropeanCups");

                await SafeDeleteAsync(() => _context.Competitions
                    .Where(c => seasons.Contains(c.SeasonId)).ExecuteDeleteAsync(), "Competitions");

                // =============== 5️⃣ Основни таблици ===============
                await SafeDeleteAsync(() => _context.Players
                    .Where(p => p.GameSaveId == saveId).ExecuteDeleteAsync(), "Players");

                await SafeDeleteAsync(() => _context.Seasons
                    .Where(s => s.GameSaveId == saveId).ExecuteDeleteAsync(), "Seasons");

                await SafeDeleteAsync(() => _context.FinancialTransactions
                    .Where(ft => ft.FromAgency.GameSaveId == saveId || ft.ToAgency.GameSaveId == saveId)
                    .ExecuteDeleteAsync(), "FinancialTransactions (Agencies)");

                await SafeDeleteAsync(() => _context.Agencies
                    .Where(a => a.GameSaveId == saveId).ExecuteDeleteAsync(), "Agencies");

                await SafeDeleteAsync(() => _context.Banks
                    .Where(b => b.GameSaveId == saveId).ExecuteDeleteAsync(), "Banks");

                // =============== 6️⃣ Финал — GameSave & Users ===============
                await SafeDeleteAsync(() => _context.GameSaves
                    .Where(gs => gs.Id == saveId).ExecuteDeleteAsync(), "GameSaves");
                await SafeDeleteAsync(() => _context.Users
                    .Where(u => u.CurrentSaveId == saveId).ExecuteDeleteAsync(), "Users");

                await transaction.CommitAsync();
                Console.WriteLine($"✅ Successfully deleted GameSave {saveId}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"💥 ERROR deleting GameSave {saveId}: {ex.Message}");
                throw;
            }
        }

        private static async Task SafeDeleteAsync(Func<Task<int>> action, string label)
        {
            try
            {
                var deleted = await action();
                Console.WriteLine($"🗑 Deleted {deleted} from {label}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed deleting from {label}: {ex.Message}");
                throw new Exception($"Error in {label}: {ex.Message}", ex);
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
