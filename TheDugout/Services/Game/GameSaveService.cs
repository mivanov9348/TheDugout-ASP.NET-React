using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services.EuropeanCup;
using TheDugout.Services.Finance;
using TheDugout.Services.Fixture;
using TheDugout.Services.League;
using TheDugout.Services.Players;
using TheDugout.Services.Season;
using TheDugout.Services.Team;

namespace TheDugout.Services.Game
{
    public class GameSaveService : IGameSaveService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<GameSaveService> _logger;
        private readonly ILeagueService _leagueGenerator;
        private readonly ISeasonGenerationService _seasonGenerator;
        private readonly IFixturesService _fixturesService;
        private readonly IPlayerGenerationService _playerGenerator;
        private readonly IFinanceService _financeService;
        private readonly ITeamGenerationService _teamGenerator;
        private readonly ITeamPlanService _teamPlanService;
        private readonly IEuropeanCupService _europeanCupService;

        public GameSaveService(
            DugoutDbContext context,
            ILogger<GameSaveService> logger,
            ILeagueService leagueGenerator,
            ISeasonGenerationService seasonGenerator,
            IFixturesService fixturesService,
            IPlayerGenerationService playerGenerator,
            IFinanceService financeService,
            ITeamPlanService teamPlanService,
            IEuropeanCupService europeanCupService,
            ITeamGenerationService teamGenerator
        )
        {
            _context = context;
            _logger = logger;
            _leagueGenerator = leagueGenerator;
            _seasonGenerator = seasonGenerator;
            _fixturesService = fixturesService;
            _playerGenerator = playerGenerator;
            _financeService = financeService;
            _teamPlanService = teamPlanService;
            this._europeanCupService = europeanCupService;
            _teamGenerator = teamGenerator;
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
        public async Task<bool> DeleteGameSaveAsync(int userId, int saveId)
        {
            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(gs => gs.Id == saveId && gs.UserId == userId);

            if (gameSave == null) return false;

            _context.GameSaves.Remove(gameSave);
            await _context.SaveChangesAsync();
            return true;
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

                // 3. Създаваме първи сезон
                var startDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
                var season = _seasonGenerator.GenerateSeason(gameSave, startDate);
                gameSave.Seasons.Add(season);
                await _context.SaveChangesAsync();

                // 4. Генерираме лиги + отбори
                var leagues = await _leagueGenerator.GenerateLeaguesAsync(gameSave, season);
                //foreach (var league in leagues)
                //    gameSave.Leagues.Add(league);

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
                                    .ToListAsync(ct);

                foreach (var template in euroTemplates)
                {
                    try
                    {
                        // 1. Вземаме подходящи отбори: LeagueId == null и същия GameSave
                        var eligibleTeams = await _context.Set<Models.Team>()
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

                        // 3. Генерираме league-phase fixtures
                        await _europeanCupService.GenerateLeaguePhaseFixturesAsync(cup.Id, season.Id, ct);

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


                // 5. Свободни агенти
                var freeAgents = _playerGenerator.GenerateFreeAgents(gameSave, 100);
                foreach (var agent in freeAgents)
                    _context.Players.Add(agent);

                await _context.SaveChangesAsync();

                // 6. Генерираме fixtures
                await _fixturesService.GenerateFixturesAsync(gameSave.Id, season.Id, startDate);

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
