using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TheDugout.Data;
using TheDugout.Models.Facilities;
using TheDugout.Models.Finance;
using TheDugout.Services.Finance;

namespace TheDugout.Services.Facilities
{
    public class StadiumService : IStadiumService
    {
        private readonly DugoutDbContext _context;
        private readonly IFinanceService _financeService;

        private readonly string _facilityCostsPath = "Data/SeedFiles/facilitiesUpgradeCost.json";
        private readonly string _stadiumLevelsPath = "Data/SeedFiles/stadiumLevels.json";

        public StadiumService(DugoutDbContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        public async Task AddStadiumAsync(int teamId)
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
                TeamId = team.Id,
                Level = 1,
                Capacity = level1.Capacity,
                GameSaveId = gameSave.Id,
                TicketPrice = level1.TicketPrice
            };

            _context.Stadiums.Add(stadium);
            await _context.SaveChangesAsync();
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
            var transaction = await _financeService.ClubToBankAsync(
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
