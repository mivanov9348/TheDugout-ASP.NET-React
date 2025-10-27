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
        private readonly ILogger<CupResultService> _logger;
        public CupResultService(DugoutDbContext context, IMoneyPrizeService moneyPrizeService, ILogger<CupResultService> logger)
        {
            _context = context;
            _moneyPrizeService = moneyPrizeService;
            _logger = logger;
        }
        public async Task<List<CompetitionSeasonResult>> GenerateCupResultsAsync(int seasonId)
        {
            bool alreadyExists = await _context.CompetitionSeasonResults
                .AnyAsync(r => r.SeasonId == seasonId && r.CompetitionType == CompetitionTypeEnum.DomesticCup);

            if (alreadyExists)
                return new List<CompetitionSeasonResult>();

            var gameSave = _context.GameSaves
                .FirstOrDefault(gs => gs.CurrentSeasonId == seasonId);

            if (gameSave == null)
                throw new Exception("Game Save is null!");

            var alreadyQualified = new HashSet<int>(
                await _context.CompetitionEuropeanQualifiedTeams
                    .Where(q => q.GameSaveId == gameSave.Id && q.CompetitionSeasonResult.SeasonId == seasonId)
                    .Select(q => q.TeamId)
                    .ToListAsync()
            );

            var cups = await _context.Cups
                .Include(c => c.Country)
                .Include(c => c.Template)
                .Include(c => c.Competition)
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
                             

                // 🏆 Резултатен запис
                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.DomesticCup,
                    Competition = cup.Competition,
                    CompetitionId = cup.CompetitionId,
                    GameSaveId = cup.GameSaveId,
                    ChampionTeamId = championTeamId,
                    RunnerUpTeamId = runnerUpTeamId,
                    Notes = $"Купа {cup.Template.Name} ({cup.Country.Name}) - Финал: {finalMatch.HomeTeam?.Name} {finalMatch.HomeTeamGoals}:{finalMatch.AwayTeamGoals} {finalMatch.AwayTeam?.Name}"
                };

                // 💰 Награди за шампиона и финалиста
                if (championTeamId.HasValue)
                {
                    // Проверка дали шампионът вече е класиран за Европа
                    if (alreadyQualified.Contains(championTeamId.Value))
                    {
                        // Намираме следващия отбор от същата държава, който не е класиран
                        var league = await _context.Leagues
                            .Include(l => l.Standings).ThenInclude(s => s.Team)
                            .Where(l => l.CountryId == cup.CountryId && l.SeasonId == seasonId && l.Tier == 1)
                            .FirstOrDefaultAsync();

                        if (league != null)
                        {
                            var orderedStandings = league.Standings
                                .OrderByDescending(s => s.Points)
                                .ThenByDescending(s => s.GoalDifference)
                                .ThenByDescending(s => s.GoalsFor)
                                .ToList();

                            var replacement = orderedStandings
                                .Select(s => s.Team)
                                .FirstOrDefault(t => !alreadyQualified.Contains(t.Id));

                            if (replacement != null)
                            {
                                _logger.LogInformation("Куповият шампион {ChampionId} вече е класиран. Даваме квотата на {Replacement}.",
                                    championTeamId.Value, replacement.Name);

                                championTeamId = replacement.Id;
                                result.ChampionTeamId = replacement.Id;
                            }
                        }
                    }

                    // Добави шампиона (или заместника) като квалифициран
                    //result.EuropeanQualifiedTeams.Add(new CompetitionEuropeanQualifiedTeam
                    //{
                    //    TeamId = championTeamId.Value,
                    //    GameSaveId = cup.GameSaveId
                    //});

                    _context.CompetitionEuropeanQualifiedTeams.Add(new CompetitionEuropeanQualifiedTeam
                    {
                        TeamId = championTeamId.Value,
                        GameSaveId = cup.GameSaveId,
                        CompetitionSeasonResult = result
                    });

                    alreadyQualified.Add(championTeamId.Value);

                    var champion = finalMatch.HomeTeamId == championTeamId
                        ? finalMatch.HomeTeam
                        : finalMatch.AwayTeam;

                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        "CUP_CHAMPION",
                        champion,
                        $"Награда за спечелване на Купа {cup.Template.Name}"
                    );
                }

                if (runnerUpTeamId.HasValue)
                {
                    var runnerUp = finalMatch.HomeTeamId == runnerUpTeamId
                        ? finalMatch.HomeTeam
                        : finalMatch.AwayTeam;

                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        "CUP_RUNNER_UP",
                        runnerUp,
                        $"Награда за финал в Купа {cup.Template.Name}"
                    );
                }

                results.Add(result);
            }

            return results;
        }
    }
}
