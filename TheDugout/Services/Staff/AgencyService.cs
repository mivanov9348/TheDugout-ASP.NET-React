namespace TheDugout.Services
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Game;
    using TheDugout.Models.Staff;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Staff.Interfaces;

    public class AgencyService : IAgencyService
    {
        private readonly DugoutDbContext _context;
        private readonly IPlayerGenerationService _playerGenerationService;
        private readonly IAgencyFinanceService _agencyFinanceService;
        private readonly Random _rng = new();
        private readonly ILogger<AgencyService> _logger;
        public AgencyService(DugoutDbContext context, IPlayerGenerationService playerGenerationService, IAgencyFinanceService agencyFinanceService, ILogger<AgencyService> logger)
        {
            _context = context;
            _playerGenerationService = playerGenerationService;
            _agencyFinanceService = agencyFinanceService;
            _logger = logger;
        }

        public async Task InitializeAgenciesForGameSaveAsync(GameSave save, CancellationToken ct = default)
        {
            var agencies = await InitializeAgenciesAsync(save, ct);
            await _playerGenerationService.GeneratePlayersForAgenciesAsync(save, agencies, ct);
        }

        public async Task<List<Agency>> InitializeAgenciesAsync(GameSave save, CancellationToken ct = default)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));

            var templates = await _context.AgencyTemplates
                .Where(t => t.IsActive)
                .ToListAsync(ct);

            if (!templates.Any())
                throw new InvalidOperationException("No AgencyTemplates found.");

            var regions = await _context.Regions.ToListAsync(ct);
            var agencies = new List<Agency>();

            foreach (var template in templates)
            {
                var regionId = regions
                    .Where(r => r.Code == template.RegionCode)
                    .Select(r => r.Id)
                    .FirstOrDefault();

                var agency = new Agency
                {
                    AgencyTemplateId = template.Id,
                    GameSaveId = save.Id,
                    TotalEarnings = 0,
                    Logo = $"{template.Name}.png",
                    IsActive = true,
                    RegionId = regionId,
                    Popularity = _rng.Next(1, 4)
                };

                agencies.Add(agency);
                _context.Agencies.Add(agency);
            }

            await _context.SaveChangesAsync(ct);

            foreach (var agency in agencies)
            {
                await _agencyFinanceService.InitializeAgencyFundsAsync(save, agency);
            }

            return agencies;
        }
        public async Task DistributeSolidarityPaymentsAsync(GameSave save, decimal percentage)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (percentage <= 0) return;

            var previousSeason = await _context.Seasons
                .Where(s => s.GameSaveId == save.Id && !s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (previousSeason == null)
            {
                _logger.LogWarning("No previous season found for GameSave {GameSaveId}", save.Id);
                return;
            }

            var totalTransferFees = await _context.FinancialTransactions
                .Where(t => t.GameSaveId == save.Id &&
                            t.SeasonId == previousSeason.Id &&
                            t.Type == TransactionType.TransferFee &&
                            t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.Amount);

            // използваме параметъра percentage (примерно 20 → 0.20)
            var percentageDecimal = percentage / 100m;
            var solidarityPool = totalTransferFees * percentageDecimal;

            if (solidarityPool <= 0)
            {
                _logger.LogInformation("No transfer fees found for previous season {SeasonId}", previousSeason.Id);
                return;
            }

            var agencies = await _context.Agencies
                .Where(a => a.GameSaveId == save.Id && a.IsActive)
                .ToListAsync();

            if (agencies.Count == 0)
            {
                _logger.LogWarning("No agencies found for GameSave {GameSaveId}", save.Id);
                return;
            }

            var perAgency = solidarityPool / agencies.Count;
            var bank = await _context.Banks.FirstAsync(b => b.GameSaveId == save.Id);

            foreach (var agency in agencies)
            {
                agency.Budget += perAgency;
                bank.Balance -= perAgency;

                _context.FinancialTransactions.Add(new FinancialTransaction
                {
                    BankId = bank.Id,
                    ToAgencyId = agency.Id,
                    GameSaveId = save.Id,
                    SeasonId = previousSeason.Id,
                    Amount = perAgency,
                    Type = TransactionType.Prize,
                    Status = TransactionStatus.Completed,
                    Description = $"Solidarity payment ({percentageDecimal:P0}) for season {previousSeason.Id}"
                });
            }

            await _context.SaveChangesAsync();
        }

    }
}
