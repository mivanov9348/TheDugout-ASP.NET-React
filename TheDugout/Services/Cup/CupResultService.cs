namespace TheDugout.Services.Cup
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Services.Cup.Interfaces;
    public class CupResultService : ICupResultService
    {
        private readonly DugoutDbContext _context;
        public CupResultService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompetitionSeasonResult>> GenerateCupResultsAsync(int seasonId)
        {
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
                // Намираме финалния рунд
                var finalRound = cup.Rounds
                    .OrderByDescending(r => r.RoundNumber)
                    .FirstOrDefault();

                if (finalRound == null)
                    continue;

                // Взимаме финалния мач
                var finalMatch = finalRound.Fixtures
                    .Where(f => f.Status == MatchStageEnum.Played)
                    .OrderByDescending(f => f.Date)
                    .FirstOrDefault();

                if (finalMatch == null)
                    continue;

                // Определяме шампиона и финалиста чрез WinnerTeamId
                int? championTeamId = finalMatch.WinnerTeamId;
                int? runnerUpTeamId = null;

                if (championTeamId == finalMatch.HomeTeamId)
                    runnerUpTeamId = finalMatch.AwayTeamId;
                else if (championTeamId == finalMatch.AwayTeamId)
                    runnerUpTeamId = finalMatch.HomeTeamId;

                // fallback ако WinnerTeamId липсва
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

                // Създаваме CompetitionSeasonResult
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

                // Купата носи квота за Европа
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

            if (results.Any())
            {
                await _context.CompetitionSeasonResults.AddRangeAsync(results);
                await _context.SaveChangesAsync();
            }

            return results;
        }
    }
}
