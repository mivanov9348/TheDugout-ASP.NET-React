namespace TheDugout.Services.Facilities
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using TheDugout.Data;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Finance;
    using TheDugout.Services.Finance.Interfaces;

    public class YouthAcademyService : IYouthAcademyService
    {
        private readonly DugoutDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly string _academyLevelsPath = "Data/SeedFiles/youthAcademyTalent.json";
        private readonly string _facilityCostsPath = "Data/SeedFiles/facilitiesUpgradeCost.json";

        public YouthAcademyService(DugoutDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }
        public async Task<YouthAcademy> AddYouthAcademyAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) throw new Exception("Team not found");

            var gameSave = await _context.GameSaves.FindAsync(team.GameSaveId);
            if (gameSave == null) throw new Exception("Game save not found");

            var levelsJson = await File.ReadAllTextAsync(_academyLevelsPath);
            var levels = JsonSerializer.Deserialize<Dictionary<string, YouthAcademyLevelConfig>>(levelsJson);

            if (levels == null || !levels.ContainsKey("1"))
                throw new Exception("Youth academy level 1 not found in config");

            var level1 = levels["1"];

            var academy = new YouthAcademy
            {
                TeamId = teamId,
                GameSaveId = gameSave.Id,
                Level = 1
            };

            _context.YouthAcademies.Add(academy);
            await _context.SaveChangesAsync();

            return academy;
        }

        public async Task<bool> UpgradeYouthAcademyAsync(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.YouthAcademy)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null) throw new Exception("Team not found");
            if (team.YouthAcademy == null) throw new Exception("Youth academy not found");

            var academy = team.YouthAcademy;
            var currentLevel = academy.Level;

            if (currentLevel >= 10)
                throw new Exception("Youth academy is already at max level");

            // Load costs
            var costsJson = await File.ReadAllTextAsync(_facilityCostsPath);
            var root = JsonSerializer.Deserialize<FacilityCostRoot>(costsJson);

            if (root == null ||
                !root.FacilityCosts.ContainsKey("YouthAcademy") ||
                !root.FacilityCosts["YouthAcademy"].ContainsKey((currentLevel + 1).ToString()))
                throw new Exception("Upgrade cost not found");

            var upgradeCost = root.FacilityCosts["YouthAcademy"][(currentLevel + 1).ToString()];

            // Load new level data
            var levelsJson = await File.ReadAllTextAsync(_academyLevelsPath);
            var levels = JsonSerializer.Deserialize<Dictionary<string, YouthAcademyLevelConfig>>(levelsJson);

            if (levels == null || !levels.ContainsKey((currentLevel + 1).ToString()))
                throw new Exception("Youth academy level data not found");

            var newLevelData = levels[(currentLevel + 1).ToString()];

            // Finance
            var bank = await _context.Banks.FirstAsync();
            var transaction = await _transactionService.ClubToBankAsync(
                team,
                bank,
                upgradeCost,
                $"Youth academy upgrade to level {currentLevel + 1}",
                TransactionType.FacilityUpgrade
            );

            if (transaction.Status != TransactionStatus.Completed)
                return false;

            academy.Level++;
            await _context.SaveChangesAsync();

            return true;
        }


        public class YouthAcademyLevelConfig
        {
            public int Level { get; set; }
            public int PlayersPerYear { get; set; }
            public double PotentialMultiplier { get; set; }
        }

        public class FacilityCostRoot
        {
            public Dictionary<string, Dictionary<string, int>> FacilityCosts { get; set; } = new();
        }
    }
}
