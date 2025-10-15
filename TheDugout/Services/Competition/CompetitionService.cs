namespace TheDugout.Services.Competition
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Seasons;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.League.Interfaces;

    public class CompetitionService : ICompetitionService
    {
        private readonly DugoutDbContext _context;
        private readonly ILeagueService _leagueService;
        private readonly ICupService _cupService;
        private readonly IEuropeanCupService _euroService;
        public CompetitionService(DugoutDbContext context, ILeagueService leagueService, ICupService cupService, IEuropeanCupService euroService)
        {
            _context = context;
            _leagueService = leagueService;
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

        // Генерира финални резултати за дадено състезание
        public async Task<List<CompetitionSeasonResult>> GenerateSeasonResultAsync(int seasonId)
        {
            var leagueResults = await _leagueService.GenerateLeagueResultsAsync(seasonId);
            var cupResults = await _cupService.GenerateCupResultsAsync(seasonId);
            var euroResults = await _euroService.GenerateEuropeanCupResultsAsync(seasonId);

            var allResults = new List<CompetitionSeasonResult>();
            allResults.AddRange(leagueResults);
            allResults.AddRange(cupResults);
            allResults.AddRange(euroResults);

            return allResults;
        }

        // Управлява промоции и изпадания след приключване на сезона
        public async Task ProcessPromotionAndRelegationAsync(int seasonId)
        {
            // TODO: 
            // 1️⃣ Извлечи всички лиги в сезона
            // 2️⃣ Намери завършилите (IsFinished = true)
            // 3️⃣ Извикай LeagueResultBuilder → връща резултати (кой изпада/кой се качва)
            // 4️⃣ Актуализирай следващия сезон:
            //     - премести отборите между лиги
            //     - създай CompetitionSeasonResult записи
            throw new NotImplementedException();
        }
    }
}
