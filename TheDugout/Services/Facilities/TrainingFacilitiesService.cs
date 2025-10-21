namespace TheDugout.Services.Facilities
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using TheDugout.Data;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Finance;
    using TheDugout.Services.Finance.Interfaces;
    using static TheDugout.Services.Facilities.StadiumService;
    public class TrainingFacilitiesService : ITrainingFacilitiesService
    {
        private readonly DugoutDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly string _trainingLevelsPath = "Data/SeedFiles/trainingQuality.json";
        private readonly string _facilityCostsPath = "Data/SeedFiles/facilitiesUpgradeCost.json";
        public TrainingFacilitiesService(DugoutDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }
        public async Task<TrainingFacility> AddTrainingFacilityAsync(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) throw new Exception("Team not found");

            var game = await _context.GameSaves.FindAsync(team.GameSaveId);
            if (game == null) throw new Exception("Game save not found");

            var levelsJson = await File.ReadAllTextAsync(_trainingLevelsPath);
            var levels = JsonSerializer.Deserialize<TrainingLevelsRoot>(levelsJson);

            if (levels == null || !levels.TrainingLevels.ContainsKey("1"))
                throw new Exception("Training level 1 not found in config");

            var level1 = levels.TrainingLevels["1"];

            var trainingFacility = new TrainingFacility
            {
                TeamId = teamId,
                GameSaveId = game.Id,
                Level = 1,
                TrainingQuality = level1.TrainingQuality
            };

            _context.TrainingFacilities.Add(trainingFacility);
            await _context.SaveChangesAsync();

            return trainingFacility;
        }

        public async Task<bool> UpgradeTrainingFacilityAsync(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.TrainingFacility)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null) throw new Exception("Team not found");
            if (team.TrainingFacility == null) throw new Exception("Training facility not found");

            var facility = team.TrainingFacility;
            var currentLevel = facility.Level;

            if (currentLevel >= 10)
                throw new Exception("Training facility is already at max level");

            
            var costsJson = await File.ReadAllTextAsync(_facilityCostsPath);
            var costs = JsonSerializer.Deserialize<FacilityCostsRoot>(costsJson);
            if (costs == null || !costs.FacilityCosts["TrainingFacility"].ContainsKey((currentLevel + 1).ToString()))
                throw new Exception("Upgrade cost not found");

            var upgradeCost = costs.FacilityCosts["TrainingFacility"][(currentLevel + 1).ToString()];

            var levelsJson = await File.ReadAllTextAsync(_trainingLevelsPath);
            var levels = JsonSerializer.Deserialize<TrainingLevelsRoot>(levelsJson);
            if (levels == null || !levels.TrainingLevels.ContainsKey((currentLevel + 1).ToString()))
                throw new Exception("Training level data not found");

            var newLevelData = levels.TrainingLevels[(currentLevel + 1).ToString()];

            var bank = await _context.Banks.FirstAsync();
            var transaction = await _transactionService.ClubToBankAsync(
                team,
                bank,
                upgradeCost,
                $"Training facility upgrade to level {currentLevel + 1}",
                TransactionType.FacilityUpgrade
            );

            if (transaction.Status != TransactionStatus.Completed)
                return false;

            facility.Level++;
            facility.TrainingQuality = newLevelData.TrainingQuality;

            await _context.SaveChangesAsync();
            return true;
        }

        public class TrainingLevelsRoot
        {
            public Dictionary<string, TrainingLevelData> TrainingLevels { get; set; } = new();
        }

        public class TrainingLevelData
        {
            public int TrainingQuality { get; set; }

        }

    }
}
