namespace TheDugout.Services.Season
{
    using Bogus;
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.EuropeanCup;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.League;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Season.Interfaces;
    using TheDugout.Services.Staff.Interfaces;

    public class NewSeasonService : INewSeasonService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<NewSeasonService> _logger;
        private readonly ICupService _cupService;
        private readonly ISeasonCleanupService _seasonCleanupService;
        private readonly ILeagueService _leagueService;
        private readonly IEuropeanCupService _europeanCupService;
        private readonly ILeagueFixturesService _leagueFixturesService;
        private readonly IPlayerGenerationService _playerGenerationService;
        public NewSeasonService(DugoutDbContext context, ILogger<NewSeasonService> logger, ICupService cupService, ISeasonCleanupService seasonCleanupService, ILeagueService leagueService, IEuropeanCupService europeanCupService, ILeagueFixturesService leagueFixturesService, IPlayerGenerationService playerGenerationService)
        {
            _context = context;
            _logger = logger;
            _cupService = cupService;
            _seasonCleanupService = seasonCleanupService;
            _leagueService = leagueService;
            _europeanCupService = europeanCupService;
            _leagueFixturesService = leagueFixturesService;
            _playerGenerationService = playerGenerationService;
        }
        public async Task<Season> GenerateSeason(GameSave gameSave, DateTime startDate)
        {
            var season = new Season
            {
                GameSaveId = gameSave.Id,
                StartDate = startDate,
                EndDate = startDate.AddYears(1).AddDays(-1),
                CurrentDate = startDate,
                IsActive = true
            };

            // Save the new season to get its ID
            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

            // Update the GameSave with the new season ID
            gameSave.CurrentSeasonId = season.Id;
            await _context.SaveChangesAsync();

            // Create season events
            var events = new List<SeasonEvent>();
            var currentDate = season.StartDate;

            while (currentDate <= season.EndDate)
            {
                events.Add(new SeasonEvent
                {
                    SeasonId = season.Id,
                    Date = currentDate,
                    Type = GetEventType(currentDate, season.StartDate, season.EndDate),
                    Description = GetDescription(currentDate, season.StartDate, season.EndDate),
                    GameSaveId = gameSave.Id,
                    IsOccupied = false
                });
                currentDate = currentDate.AddDays(1);
            }

            _context.SeasonEvents.AddRange(events);
            await _context.SaveChangesAsync();

            return season;
        }
        public async Task<bool> StartNewSeasonAsync(int seasonId)
        {
            _logger.LogInformation("🚀 [StartNewSeasonAsync] Starting new season after {SeasonId}", seasonId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Clean up old season data
                await _seasonCleanupService.CleanupOldSeasonDataAsync(seasonId);

                var previousSeason = await _context.Seasons
                                    .Include(s => s.GameSave)
                                    .FirstAsync(s => s.Id == seasonId);

                var gameSave = _context.GameSaves
                        .FirstOrDefault(gs => gs.CurrentSeasonId == seasonId);

                if (gameSave == null)
                {
                    throw new Exception("Game Save is null!");
                }

                // New Season
                var newSeason = await GenerateSeason(gameSave, previousSeason.EndDate.AddDays(1));
                _logger.LogInformation("✅ Created new Season {Id}", newSeason.Id);

                // New leagues with new releagated/promoted
                var newLeagues = await _leagueService.GenerateLeaguesAsync(gameSave, newSeason);

                await _leagueService.CopyTeamsFromPreviousSeasonAsync(previousSeason, newLeagues);
                _logger.LogInformation("Copied all teams from previous season to new season leagues");

                await _leagueService.ProcessPromotionsAndRelegationsAsync(previousSeason.GameSave, previousSeason, newLeagues);
                _logger.LogInformation("Processed promotions and relegations");

                // 5️⃣ Създай standings за всички лиги
                await _leagueService.InitializeStandingsAsync(previousSeason.GameSave, newSeason);
                _logger.LogInformation("Initialized new standings");

                // new eurocup for season with qualified from previous season + random other teams
                var euroTemplates = await _context.Set<EuropeanCupTemplate>()
                                    .Include(t => t.PhaseTemplates)
                                    .Where(t => t.IsActive)
                                    .ToListAsync();
                _logger.LogInformation("Creating new European Cups for Season {SeasonId}", newSeason.Id);

                foreach (var template in euroTemplates)
                {
                    try
                    {
                        await _europeanCupService.InitializeTournamentAsync(
                            templateId: template.Id,
                            gameSaveId: gameSave.Id,
                            seasonId: newSeason.Id,
                            previousSeasonId: previousSeason.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to initialize European Cup template '{TemplateName}' ({TemplateId}) for new season",
                            template.Name, template.Id);
                        // Помисли дали да не хвърлиш грешката нагоре, за да предизвикаш rollback
                    }
                }
                // new domestic cups same rules
                await _cupService.InitializeCupsForGameSaveAsync(gameSave, newSeason.Id);
                _logger.LogInformation("Initialized Domestic Cups");

                // fixtures same rules
                await _leagueFixturesService.GenerateLeagueFixturesAsync(gameSave.Id, newSeason.Id, newSeason.StartDate);
                _logger.LogInformation("Generated League Fixtures");

                // agencies
                var agencies = _context.Agencies.Where(a => a.GameSaveId == gameSave.Id).ToList();
                await _playerGenerationService.GeneratePlayersForAgenciesAsync(gameSave, agencies);

                // 💾 7. Save and Commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("🎉 New season successfully started!");
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [StartNewSeasonAsync] Error occurred, rolling back transaction.");
                await transaction.RollbackAsync();
                return false;
            }
        }

        private SeasonEventType GetEventType(DateTime date, DateTime seasonStart, DateTime seasonEnd)
        {
            if (date.Date == seasonStart.Date)
                return SeasonEventType.StartSeason;

            if (date.Date == seasonEnd.Date)
                return SeasonEventType.EndOfSeason;

            // First 7 days of the season = transfer window
            if (date >= seasonStart && date < seasonStart.AddDays(7))
                return SeasonEventType.TransferWindow;

            // Middle 7 days of the season = transfer window
            var midSeason = seasonStart.AddDays((seasonEnd - seasonStart).Days / 2);
            if (date >= midSeason && date < midSeason.AddDays(7))
                return SeasonEventType.TransferWindow;

            // Weekly events
            return date.DayOfWeek switch
            {
                DayOfWeek.Tuesday => SeasonEventType.EuropeanMatch,
                DayOfWeek.Thursday => SeasonEventType.CupMatch,
                DayOfWeek.Saturday => SeasonEventType.ChampionshipMatch,
                _ => SeasonEventType.TrainingDay
            };
        }
        private string GetDescription(DateTime date, DateTime seasonStart, DateTime seasonEnd) =>
            GetEventType(date, seasonStart, seasonEnd) switch
            {
                SeasonEventType.StartSeason => "Start of New Season",
                SeasonEventType.EndOfSeason => "End of the Season",
                SeasonEventType.TransferWindow => "Transfer Window",
                SeasonEventType.ChampionshipMatch => "League Matchday",
                SeasonEventType.CupMatch => "Cup Match",
                SeasonEventType.EuropeanMatch => "European Match",
                SeasonEventType.TrainingDay => "Training Day",
                _ => "Other"
            };
    }
}
