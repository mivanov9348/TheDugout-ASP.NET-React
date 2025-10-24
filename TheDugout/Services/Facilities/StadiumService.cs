namespace TheDugout.Services.Facilities
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using TheDugout.Data;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Matches;
    using TheDugout.Services.Finance.Interfaces;

    public class StadiumService : IStadiumService
    {
        private readonly DugoutDbContext _context;
        private readonly ITransactionService _transactionService;
        private const int MAX_LEVEL = 10;

        private readonly string _facilityCostsPath = "Data/SeedFiles/facilitiesUpgradeCost.json";
        private readonly string _stadiumLevelsPath = "Data/SeedFiles/stadiumLevels.json";

        public StadiumService(DugoutDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<Stadium> AddStadiumAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) throw new Exception("Team not found");

            var gameSave = await _context.GameSaves.FindAsync(team.GameSaveId);

            if (gameSave == null) throw new Exception("Game save not found");

            var levelsJson = await File.ReadAllTextAsync(_stadiumLevelsPath);
            var levels = JsonSerializer.Deserialize<StadiumLevelsRoot>(levelsJson);

            if (levels == null || !levels.StadiumLevels.ContainsKey("1"))
                throw new Exception("Stadium level 1 not found in config");

            var level1 = levels.StadiumLevels["1"];

            var stadium = new Stadium
            {
                TeamId = teamId,
                Level = 1,
                Capacity = level1.Capacity,
                GameSaveId = gameSave.Id,
                TicketPrice = level1.TicketPrice
            };

            _context.Stadiums.Add(stadium);
            await _context.SaveChangesAsync();

            return stadium;
        }
        public async Task<bool> UpgradeStadiumAsync(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Stadium)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null) throw new Exception("Team not found");
            if (team.Stadium == null) throw new Exception("Stadium not found");

            var stadium = team.Stadium;
            var currentLevel = stadium.Level;

            if (currentLevel >= 10)
                throw new Exception("Stadium is already at max level");

            var costsJson = await File.ReadAllTextAsync(_facilityCostsPath);
            var costs = JsonSerializer.Deserialize<FacilityCostsRoot>(costsJson);
            if (costs == null || !costs.FacilityCosts["Stadium"].ContainsKey((currentLevel + 1).ToString()))
                throw new Exception("Upgrade cost not found");

            var upgradeCost = costs.FacilityCosts["Stadium"][(currentLevel + 1).ToString()];

            var levelsJson = await File.ReadAllTextAsync(_stadiumLevelsPath);
            var levels = JsonSerializer.Deserialize<StadiumLevelsRoot>(levelsJson);
            if (levels == null || !levels.StadiumLevels.ContainsKey((currentLevel + 1).ToString()))
                throw new Exception("Stadium level data not found");

            var newLevelData = levels.StadiumLevels[(currentLevel + 1).ToString()];

            var bank = await _context.Banks.FirstAsync();
            var transaction = await _transactionService.ClubToBankAsync(
                team,
                bank,
                upgradeCost,
                $"Stadium upgrade to level {currentLevel + 1}",
                TransactionType.FacilityUpgrade
            );

            if (transaction.Status != TransactionStatus.Completed)
                return false;

            stadium.Level++;
            stadium.Capacity = newLevelData.Capacity;
            stadium.TicketPrice = newLevelData.TicketPrice;

            await _context.SaveChangesAsync();
            return true;
        }
        public long? GetNextUpgradeCost(int currentLevel)
        {
            if (currentLevel >= MAX_LEVEL)
            {
                return null; // Няма ъпгрейд след макс. ниво
            }

            int nextLevel = currentLevel + 1;
            string nextLevelStr = nextLevel.ToString();

            try
            {
                var costsJson = File.ReadAllText(_facilityCostsPath);
                var costs = JsonSerializer.Deserialize<FacilityCostsRoot>(costsJson);

                if (costs != null &&
                    costs.FacilityCosts.TryGetValue("Stadium", out var stadiumCosts) &&
                    stadiumCosts.TryGetValue(nextLevelStr, out decimal decimalCost))
                {
                    // (long)decimalCost е правилно,
                    // защото твоят модел FacilityCostsRoot използва 'decimal'
                    return (long)decimalCost;
                }
            }
            catch (Exception)
            {
                // Грешка при четене/парсване на файла
                return null;
            }

            return null; // Не е намерена цена
        }

        public async Task<decimal> GenerateMatchRevenueAsync(Match match)
        {
            var team = await _context.Teams
                .Include(t => t.Stadium)
                .FirstOrDefaultAsync(t => t.Id == match.Fixture.HomeTeamId);

            if (team == null)
                throw new Exception("Home team not found");

            if (team.Stadium == null)
                throw new Exception("Stadium not found");

            var stadium = team.Stadium;
            var attendance = match.Attendance; 

            // 💰 Приход от билети
            decimal revenue = attendance * stadium.TicketPrice;

            // 💵 Външен приход от публиката
            await _transactionService.ExternalToClubAsync(
                team,
                revenue,
                $"{match.Fixture.HomeTeam?.Name} vs {match.Fixture.AwayTeam?.Name}: Ticket sales from {attendance} spectators",
                TransactionType.MatchIncome
            );

            return revenue;
        }

        public class FacilityCostsRoot
        {
            public Dictionary<string, Dictionary<string, decimal>> FacilityCosts { get; set; } = new();
        }

        public class StadiumLevelsRoot
        {
            public Dictionary<string, StadiumLevelData> StadiumLevels { get; set; } = new();
        }

        public class StadiumLevelData
        {
            public int Capacity { get; set; }
            public decimal TicketPrice { get; set; }
        }
    }
}
