namespace TheDugout.Services.EuropeanCup
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;

    public class EuropeanCupResultService : IEuropeanCupResultService
    {
        private readonly DugoutDbContext _context;
        private readonly IMoneyPrizeService _moneyPrizeService;
        private readonly ILogger<EuroCupTeamService> _logger;
        public EuropeanCupResultService(DugoutDbContext context, IMoneyPrizeService moneyPrizeService, ILogger<EuroCupTeamService> logger)
        {
            _context = context;
            _moneyPrizeService = moneyPrizeService;
            _logger = logger;
        }
        public async Task<List<CompetitionSeasonResult>> GenerateEuropeanCupResultsAsync(int seasonId)
        {
            // Проверка дали вече има записани резултати
            bool alreadyExists = await _context.CompetitionSeasonResults
                .AnyAsync(r => r.SeasonId == seasonId && r.CompetitionType == CompetitionTypeEnum.EuropeanCup);

            if (alreadyExists)
            {
                _logger.LogInformation("⚠️ European cup results already exist for season {SeasonId}", seasonId);
                return new List<CompetitionSeasonResult>();
            }

            var gameSave = await _context.GameSaves
                .FirstOrDefaultAsync(gs => gs.CurrentSeasonId == seasonId);

            if (gameSave == null)
                throw new Exception("No Game Save found!");

            // Зареждаме всички еврокупи, които са приключили
            var europeanCups = await _context.EuropeanCups
                .Include(e => e.Template)
                .Include(e => e.Phases)
                    .ThenInclude(p => p.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                .Include(e => e.Phases)
                    .ThenInclude(p => p.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                .Include(e => e.Competition)
                // ⚠️ Без AsNoTracking(), за да може EF да track-ва навигациите
                .Where(e => e.SeasonId == seasonId && e.IsFinished)
                .ToListAsync();

            _logger.LogInformation("🔎 Found {Count} finished european cups for season {SeasonId}", europeanCups.Count, seasonId);

            var results = new List<CompetitionSeasonResult>();

            foreach (var euro in europeanCups)
            {
                // Намираме финалната фаза
                var finalPhase = await _context.EuropeanCupPhases
                    .Include(ph => ph.PhaseTemplate)
                    .Include(ph => ph.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                    .Include(ph => ph.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                    .Where(ph => ph.EuropeanCupId == euro.Id)
                    .OrderByDescending(ph => ph.PhaseTemplate.Order)
                    .FirstOrDefaultAsync();

                if (finalPhase == null)
                {
                    _logger.LogWarning("⚠️ No final phase found for European Cup {CupId}", euro.Id);
                    continue;
                }

                // Финален мач
                var finalMatch = finalPhase.Fixtures
                    .Where(f => f.Status == MatchStageEnum.Played)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                if (finalMatch == null)
                {
                    _logger.LogWarning("⚠️ No final match found for European Cup {CupId}", euro.Id);
                    continue;
                }

                // Определяне на шампиона и финалиста
                int? championTeamId = finalMatch.WinnerTeamId;
                int? runnerUpTeamId = null;

                if (championTeamId == finalMatch.HomeTeamId)
                    runnerUpTeamId = finalMatch.AwayTeamId;
                else if (championTeamId == finalMatch.AwayTeamId)
                    runnerUpTeamId = finalMatch.HomeTeamId;

                if (championTeamId == null)
                {
                    if (finalMatch.HomeTeamGoals > finalMatch.AwayTeamGoals)
                    {
                        championTeamId = finalMatch.HomeTeamId;
                        runnerUpTeamId = finalMatch.AwayTeamId;
                    }
                    else
                    {
                        championTeamId = finalMatch.AwayTeamId;
                        runnerUpTeamId = finalMatch.HomeTeamId;
                    }
                }

                // 💰 Награди
                if (championTeamId.HasValue)
                {
                    var champion = finalMatch.HomeTeamId == championTeamId
                        ? finalMatch.HomeTeam
                        : finalMatch.AwayTeam;

                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        "EURO_CHAMPION",
                        champion,
                        $"Награда за спечелване на {euro.Template.Name}"
                    );
                }

                if (runnerUpTeamId.HasValue)
                {
                    var runnerUp = finalMatch.HomeTeamId == runnerUpTeamId
                        ? finalMatch.HomeTeam
                        : finalMatch.AwayTeam;

                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        "EURO_RUNNER_UP",
                        runnerUp,
                        $"Награда за финал в {euro.Template.Name}"
                    );
                }

                // 🏆 Създаваме и добавяме резултата в контекста
                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.EuropeanCup,
                    CompetitionId = euro.CompetitionId,
                    GameSaveId = euro.GameSaveId,
                    ChampionTeamId = championTeamId,
                    RunnerUpTeamId = runnerUpTeamId,
                    Notes = $"Еврокупа {euro.Template.Name} - Финал: {finalMatch.HomeTeam?.Name} {finalMatch.HomeTeamGoals}:{finalMatch.AwayTeamGoals} {finalMatch.AwayTeam?.Name}"
                };

                // Задължително зануляваме навигацията, за да не се опита да attach-не detached обект
                result.Competition = null;

                _context.CompetitionSeasonResults.Add(result);
                results.Add(result);

                _logger.LogInformation("✅ Added European Cup result for {CupName}", euro.Template.Name);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("💾 Saved {Count} European Cup results for season {SeasonId}", results.Count, seasonId);

            return results;
        }

    }
}
