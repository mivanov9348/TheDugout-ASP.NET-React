namespace TheDugout.Services.Competition
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.League.Interfaces;
    public class CompetitionService : ICompetitionService
    {
        private readonly DugoutDbContext _context;
        private readonly ILeagueService _leagueService;
        private readonly ILeagueResultService _leagueResultService;
        private readonly ICupService _cupService;
        private readonly IEuropeanCupService _euroService;
        public CompetitionService(DugoutDbContext context, ILeagueService leagueService, ILeagueResultService leagueResultService, ICupService cupService, IEuropeanCupService euroService)
        {
            _context = context;
            _leagueService = leagueService;
            _leagueResultService = leagueResultService;
            _cupService = cupService;
            _euroService = euroService;
        }
        public async Task<bool> AreAllCompetitionsFinishedAsync(int seasonId)
        {
            var leagues = await _context.Leagues.Where(l => l.SeasonId == seasonId).ToListAsync();
            var cups = await _context.Cups.Where(c => c.SeasonId == seasonId).ToListAsync();
            var euros = await _context.EuropeanCups.Where(e => e.SeasonId == seasonId).ToListAsync();

            var tasks = leagues.Select(l => _leagueService.IsLeagueFinishedAsync(l.Id))
                        .Concat(cups.Select(c => _cupService.IsCupFinishedAsync(c.Id)))
                        .Concat(euros.Select(e => _euroService.IsEuropeanCupFinishedAsync(e.Id)));

            var results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }
        public async Task<List<CompetitionSeasonResult>> GenerateSeasonResultAsync(int seasonId)
        {
            // 1️⃣ Взимаме резултатите от всички типове турнири
            var leagueResults = await _leagueResultService.GenerateLeagueResultsAsync(seasonId);
            var cupResults = await _cupService.GenerateCupResultsAsync(seasonId);
            var euroResults = await _euroService.GenerateEuropeanCupResultsAsync(seasonId);

            // Обединяваме всички резултати
            var allResults = new List<CompetitionSeasonResult>();
            allResults.AddRange(leagueResults);
            allResults.AddRange(cupResults);
            allResults.AddRange(euroResults);

            // 2️⃣ За всеки турнир намираме топ голмайстора и създаваме награда
            foreach (var result in allResults)
            {
                var competitionId = result.CompetitionId;
                var competition = await _context.Competitions
                    .FirstOrDefaultAsync(c => c.Id == competitionId);

                if (competition == null) continue;

                var topScorer = await _context.PlayerCompetitionStats
                        .Include(p => p.Player)
                        .Where(p => p.SeasonId == seasonId && p.CompetitionId == competition.Id)
                        .OrderByDescending(p => p.Goals)
                        .ThenBy(p => p.Player.LastName)
                        .FirstOrDefaultAsync();


                if (topScorer != null && topScorer.Goals > 0)
                {
                    var award = new CompetitionAward
                    {
                        AwardType = CompetitionAwardType.TopScorer,
                        PlayerId = topScorer.PlayerId!,
                        Value = topScorer.Goals,
                        CompetitionSeasonResult = result,
                        CompetitionId = competition.Id,
                        GameSaveId = competition.GameSaveId!.Value,
                        SeasonId = seasonId
                    };

                    result.Awards.Add(award);
                    _context.CompetitionAwards.Add(award);
                }

                _context.CompetitionSeasonResults.Add(result);
            }

            // 3️⃣ Запазваме всички промени
            await _context.SaveChangesAsync();

            return allResults;
        }        
    }
}
