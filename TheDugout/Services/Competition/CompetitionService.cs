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
    using TheDugout.Models.Fixtures;
    public class CompetitionService : ICompetitionService
    {
        private readonly DugoutDbContext _context;
        private readonly ILeagueService _leagueService;
        private readonly ILeagueResultService _leagueResultService;
        private readonly ICupService _cupService;
        private readonly IEuropeanCupService _euroService;
        private readonly ICupResultService _cupResultService;
        private readonly IEuropeanCupResultService _euroCupResultService;
        private readonly ILogger<CompetitionService> _logger; 
        public CompetitionService(DugoutDbContext context, ILeagueService leagueService, ILeagueResultService leagueResultService, ICupService cupService, IEuropeanCupService euroService, ICupResultService cupResultService, IEuropeanCupResultService europeanCupResultService, ILogger<CompetitionService> logger)
        {
            _context = context;
            _leagueService = leagueService;
            _leagueResultService = leagueResultService;
            _cupService = cupService;
            _euroService = euroService;
            _cupResultService = cupResultService;
            _euroCupResultService = europeanCupResultService;
            _logger = logger;
        }
        public async Task<bool> AreAllCompetitionsFinishedAsync(int seasonId)
        {
            Console.WriteLine($"🔍 Checking competitions for season {seasonId}");

            var leagues = await _context.Leagues.Where(l => l.SeasonId == seasonId).ToListAsync();
            var cups = await _context.Cups.Where(c => c.SeasonId == seasonId).ToListAsync();
            var euros = await _context.EuropeanCups.Where(e => e.SeasonId == seasonId).ToListAsync();

            Console.WriteLine($"Found {leagues.Count} leagues, {cups.Count} cups, {euros.Count} european cups.");

            foreach (var l in leagues)
            {
                if (!await _leagueService.IsLeagueFinishedAsync(l.Id))
                {
                    Console.WriteLine($"⚠️ League {l.Id} not finished yet.");
                    return false;
                }
            }

            foreach (var c in cups)
            {
                if (!await _cupService.IsCupFinishedAsync(c.Id))
                {
                    Console.WriteLine($"⚠️ Cup {c.Id} not finished yet.");
                    return false;
                }
            }

            foreach (var e in euros)
            {
                if (!await _euroService.IsEuropeanCupFinishedAsync(e.Id))
                {
                    Console.WriteLine($"⚠️ Euro cup {e.Id} not finished yet.");
                    return false;
                }
            }

            Console.WriteLine($"✅ All competitions finished for season {seasonId}");
            return true;
        }

        public async Task<List<CompetitionSeasonResult>> GenerateSeasonResultAsync(int seasonId)
        {
            // Getting results of all type of cups
            var leagueResults = await _leagueResultService.GenerateLeagueResultsAsync(seasonId);
            var cupResults = await _cupResultService.GenerateCupResultsAsync(seasonId);
            var euroResults = await _euroCupResultService.GenerateEuropeanCupResultsAsync(seasonId);

            // Collecting all results
            var allResults = new List<CompetitionSeasonResult>();
            allResults.AddRange(leagueResults);
            allResults.AddRange(cupResults);
            allResults.AddRange(euroResults);

            // For each 
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
                        .ThenByDescending(p => p.MatchesPlayed)
                        .ThenBy(p => p.Player.FirstName)
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
        public string GetCompetitionName(Fixture fixture)
        {
            try
            {
                return fixture.CompetitionType switch
                {
                    CompetitionTypeEnum.League => fixture.League?.Template?.Name ?? "Unknown League",
                    CompetitionTypeEnum.DomesticCup => fixture.CupRound?.Cup?.Template?.Name ?? "Unknown Cup",
                    CompetitionTypeEnum.EuropeanCup => fixture.EuropeanCupPhase?.EuropeanCup?.Template?.Name ?? "Unknown European Cup",
                    _ => "Unknown Competition"
                };
            }
            catch
            {
                return "Unknown Competition";
            }
        }
        public string GetCompetitionNameById(int competitionId)
        {
            var competition = _context.Competitions
                .Include(c => c.League).ThenInclude(l => l.Template)
                .Include(c => c.Cup).ThenInclude(cu => cu.Template)
                .Include(c => c.EuropeanCup).ThenInclude(ec => ec.Template)
                .FirstOrDefault(c => c.Id == competitionId);

            if (competition == null)
                return string.Empty;

            return competition.League?.Template?.Name
                ?? competition.Cup?.Template?.Name
                ?? competition.EuropeanCup?.Template?.Name
                ?? string.Empty;
        }
    }
}
