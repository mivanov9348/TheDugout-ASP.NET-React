namespace TheDugout.Services.Cup
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;

    public class CupResultService : ICupResultService
    {
        private readonly DugoutDbContext _context;
        private readonly IMoneyPrizeService _moneyPrizeService;
        public CupResultService(DugoutDbContext context, IMoneyPrizeService moneyPrizeService)
        {
            _context = context;
            _moneyPrizeService = moneyPrizeService;
        }
        public async Task<List<CompetitionSeasonResult>> GenerateCupResultsAsync(int seasonId)
        {
            bool alreadyExists = await _context.CompetitionSeasonResults
        .AnyAsync(r => r.SeasonId == seasonId && r.CompetitionType == CompetitionTypeEnum.DomesticCup);

            if (alreadyExists)
                return new List<CompetitionSeasonResult>();

            var gameSave = _context.GameSaves
                    .FirstOrDefault(gs=>gs.CurrentSeasonId == seasonId);

            if (gameSave == null)
            {
                throw new Exception("Game Save is null!");
            }

            var cups = await _context.Cups
                .Include(c => c.Country)
                .Include(c => c.Template)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                        .ThenInclude(f => f.HomeTeam)
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                        .ThenInclude(f => f.AwayTeam)
                .Where(c => c.SeasonId == seasonId && c.IsFinished)
                .ToListAsync();

            var results = new List<CompetitionSeasonResult>();

            foreach (var cup in cups)
            {
                var finalRound = cup.Rounds
                    .OrderByDescending(r => r.RoundNumber)
                    .FirstOrDefault();

                if (finalRound == null)
                    continue;

                var finalMatch = finalRound.Fixtures
                    .Where(f => f.Status == MatchStageEnum.Played)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                if (finalMatch == null)
                    continue;

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

                // 💰 Награди за шампиона и финалиста
                if (championTeamId.HasValue)
                {
                    var champion = finalMatch.HomeTeamId == championTeamId ? finalMatch.HomeTeam : finalMatch.AwayTeam;
                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        "CUP_CHAMPION",
                        champion,
                        $"Награда за спечелване на Купа {cup.Template.Name}"
                    );
                }

                if (runnerUpTeamId.HasValue)
                {
                    var runnerUp = finalMatch.HomeTeamId == runnerUpTeamId ? finalMatch.HomeTeam : finalMatch.AwayTeam;
                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        "CUP_RUNNER_UP",
                        runnerUp,
                        $"Награда за финал в Купа {cup.Template.Name}"
                    );
                }

                // 🏆 Резултатен запис
                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.DomesticCup,
                    CompetitionId = cup.CompetitionId,
                    GameSaveId = cup.GameSaveId,
                    ChampionTeamId = championTeamId,
                    RunnerUpTeamId = runnerUpTeamId,
                    Notes = $"Купа {cup.Template.Name} ({cup.Country.Name}) - Финал: {finalMatch.HomeTeam?.Name} {finalMatch.HomeTeamGoals}:{finalMatch.AwayTeamGoals} {finalMatch.AwayTeam?.Name}"
                };

                // Квота за Европа
                if (championTeamId.HasValue)
                {
                    result.EuropeanQualifiedTeams.Add(new CompetitionEuropeanQualifiedTeam
                    {
                        TeamId = championTeamId.Value,
                        GameSaveId = cup.GameSaveId
                    });
                }

                results.Add(result);
            }
            
            return results;
        }

    }
}
