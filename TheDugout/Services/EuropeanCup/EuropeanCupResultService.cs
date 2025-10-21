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
        public EuropeanCupResultService(DugoutDbContext context, IMoneyPrizeService moneyPrizeService)
        {
            _context = context;
            _moneyPrizeService = moneyPrizeService;
        }
        public async Task<List<CompetitionSeasonResult>> GenerateEuropeanCupResultsAsync(int seasonId)
        {
            var europeanCups = await _context.EuropeanCups
                .Include(e => e.Template)
                .Include(e => e.Phases)
                    .ThenInclude(p => p.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                .Include(e => e.Phases)
                    .ThenInclude(p => p.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                .Where(e => e.SeasonId == seasonId && e.IsFinished)
                .ToListAsync();

            var results = new List<CompetitionSeasonResult>();

            foreach (var euro in europeanCups)
            {
                // Последна фаза = финал
                var finalPhase = euro.Phases
                    .OrderByDescending(p => p.PhaseTemplate.Order)
                    .FirstOrDefault();

                if (finalPhase == null)
                    continue;

                // Финален мач
                var finalMatch = finalPhase.Fixtures
                    .Where(f => f.Status == MatchStageEnum.Played)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                if (finalMatch == null)
                    continue;

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
                    var champion = finalMatch.HomeTeamId == championTeamId ? finalMatch.HomeTeam : finalMatch.AwayTeam;
                    await _moneyPrizeService.GrantToTeamAsync(
                        euro.GameSave,
                        "EURO_CHAMPION",
                        champion,
                        $"Награда за спечелване на {euro.Template.Name}"
                    );
                }

                if (runnerUpTeamId.HasValue)
                {
                    var runnerUp = finalMatch.HomeTeamId == runnerUpTeamId ? finalMatch.HomeTeam : finalMatch.AwayTeam;
                    await _moneyPrizeService.GrantToTeamAsync(
                        euro.GameSave,
                        "EURO_RUNNER_UP",
                        runnerUp,
                        $"Награда за финал в {euro.Template.Name}"
                    );
                }

                // 🏆 Записваме резултата
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

                results.Add(result);
            }

            if (results.Any())
            {
                await _context.CompetitionSeasonResults.AddRangeAsync(results);
                await _context.SaveChangesAsync();
            }

            return results;
        }
    }
}
