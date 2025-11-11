namespace TheDugout.Services.Transfer
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using TheDugout.Data;
    using TheDugout.Models.Players;
    using TheDugout.Services.Transfer.Interfaces;

    public class CPUTransferService : ICPUTransferService
    {
        private readonly DugoutDbContext _context;
        private readonly IFreeAgentTransferService _freeAgentTransferService;
        private readonly ILogger<CPUTransferService> _logger;
        private readonly Random _random = new();

        public CPUTransferService(
            DugoutDbContext context,
            IFreeAgentTransferService freeAgentTransferService,
            ILogger<CPUTransferService> logger)
        {
            _context = context;
            _freeAgentTransferService = freeAgentTransferService;
            _logger = logger;
        }

        public async Task RunCpuTransfersAsync(int gameSaveId, int seasonId, DateTime date, int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Players)
                    .ThenInclude(p => p.Position)
                .Include(t => t.Players)
                    .ThenInclude(p => p.Attributes)
                        .ThenInclude(a => a.Attribute)
                .FirstOrDefaultAsync(t => t.Id == teamId && t.GameSaveId == gameSaveId);

            if (team == null)
            {
                _logger.LogWarning("❌ Team {TeamId} not found.", teamId);
                return;
            }

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null)
            {
                _logger.LogWarning("❌ No active season found for transfers.");
                return;
            }

            bool inWindow = season.Events.Any(e =>
                e.Type == Models.Enums.SeasonEventType.TransferWindow &&
                e.Date.Date == date.Date);

            if (!inWindow)
            {
                _logger.LogInformation("🚫 {TeamName}: Transfer window closed.", team.Name);
                return;
            }

            // --- 💰 Финансова логика ---
            var minTransferBudget = 100_000; // може да е динамично по дивизия
            if (team.Balance < minTransferBudget)
            {
                _logger.LogInformation($"💤 {team.Name}: Skipping market (low budget: {team.Balance:N0}€).");
                return;
            }

            var allWeights = await _context.PositionWeights
                .Include(pw => pw.Position)
                .Include(pw => pw.Attribute)
                .ToListAsync();

            if (!allWeights.Any())
            {
                _logger.LogWarning("⚠️ No position weights found.");
                return;
            }

            var players = team.Players
                .Where(p => p.Position != null && p.Attributes.Any())
                .ToList();

            var weakPositions = AnalyzeWeakPositions(players, allWeights);
            if (!weakPositions.Any())
            {
                _logger.LogInformation("🧘 {TeamName}: Squad balanced, skipping market.", team.Name);
                return;
            }

            // --- 🎯 Търси попълнения само за слаби позиции ---
            var freeAgents = await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Attributes)
                    .ThenInclude(pa => pa.Attribute)
                .Where(p => p.TeamId == null && p.GameSaveId == gameSaveId && p.IsActive)
                .ToListAsync();

            foreach (var weakPos in weakPositions)
            {
                var weights = allWeights.Where(w => w.Position.Code == weakPos).ToList();
                if (!weights.Any()) continue;

                var candidates = freeAgents
                    .Where(p => p.Position?.Code == weakPos && p.Price <= team.Balance * 0.5m)
                    .Select(p => new
                    {
                        Player = p,
                        Score = CalculateWeightedScore(p, weights)
                    })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                if (!candidates.Any())
                {
                    _logger.LogInformation($"😐 {team.Name}: No affordable candidates for {weakPos}.");
                    continue;
                }

                var best = candidates.First();
                _logger.LogInformation($"📝 {team.Name} signs {best.Player.FirstName} {best.Player.LastName} ({weakPos}) [Score {best.Score:F1}]");

                await _freeAgentTransferService.SignFreePlayer(gameSaveId, team.Id, best.Player.Id);
            }

            // --- 🧹 Освобождава излишни играчи ---
            if (team.Balance > 200_000)
            {
                var surplus = FindSurplusPlayers(players);
                foreach (var s in surplus)
                {
                    var samePosCount = players.Count(p => p.Position.Code == s.Position.Code);
                    if (samePosCount <= 2) continue; // не махай ако останат малко

                    _logger.LogInformation($"🪓 {team.Name} released {s.FirstName} {s.LastName}");
                    await _freeAgentTransferService.ReleasePlayerAsync(gameSaveId, team.Id, s.Id);
                }
            }
        }

        private List<string> AnalyzeWeakPositions(IEnumerable<Player> players, List<PositionWeight> allWeights)
        {
            var scoresByPos = players
                .Where(p => p.Position != null)
                .GroupBy(p => p.Position.Code)
                .Select(g =>
                {
                    var posWeights = allWeights.Where(w => w.Position.Code == g.Key).ToList();
                    var avgScore = g.Average(p => CalculateWeightedScore(p, posWeights));
                    return new { Pos = g.Key, Count = g.Count(), AvgScore = avgScore };
                })
                .ToList();

            return scoresByPos
                .Where(g => g.Count < 2 || g.AvgScore < 10)
                .Select(g => g.Pos)
                .ToList();
        }

        private double CalculateWeightedScore(Player player, List<PositionWeight> posWeights)
        {
            double totalWeight = 0;
            double weightedSum = 0;

            foreach (var pw in posWeights)
            {
                var attr = player.Attributes.FirstOrDefault(a => a.AttributeId == pw.AttributeId);
                if (attr == null) continue;

                weightedSum += attr.Value * pw.Weight;
                totalWeight += pw.Weight;
            }

            return totalWeight > 0 ? weightedSum / totalWeight : 0;
        }

        private List<Player> FindSurplusPlayers(IEnumerable<Player> players)
        {
            var grouped = players.GroupBy(p => p.Position.Code);
            var surplus = new List<Player>();

            foreach (var g in grouped)
            {
                if (g.Count() > 5)
                {
                    surplus.AddRange(g.OrderBy(p => p.CurrentAbility).Take(g.Count() - 5));
                }
            }

            return surplus;
        }
    }
}
