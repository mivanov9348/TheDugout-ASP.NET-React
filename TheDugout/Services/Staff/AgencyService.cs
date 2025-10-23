namespace TheDugout.Services
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
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
        public AgencyService(DugoutDbContext context, IPlayerGenerationService playerGenerationService, IAgencyFinanceService agencyFinanceService)
        {
            _context = context;
            _playerGenerationService = playerGenerationService;
            _agencyFinanceService = agencyFinanceService;
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

    }
}
