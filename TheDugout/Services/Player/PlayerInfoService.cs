namespace TheDugout.Services
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Player;
    using TheDugout.Models.Messages;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Message.Interfaces;
    using TheDugout.Services.Player.Interfaces;

    public class PlayerInfoService : IPlayerInfoService
    {
        private readonly DugoutDbContext _context;
        private readonly ICompetitionService _competitionService;
        private readonly IMessageOrchestrator _messageOrchestrator;

        public PlayerInfoService(DugoutDbContext context, ICompetitionService competitionService, IMessageOrchestrator messageOrchestrator)
        {
            _context = context;
            _competitionService = competitionService;
            _messageOrchestrator = messageOrchestrator;
        }

        public async Task<PlayerDto?> GetPlayerByIdAsync(int playerId)
        {
            return await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Country)
                .Include(p => p.Team)
                .Include(p => p.Attributes)
                    .ThenInclude(a => a.Attribute)
                .Include(p => p.SeasonStats)
                .Include(p => p.CompetitionStats)
    .ThenInclude(cs => cs.Competition)
        .ThenInclude(c => c.League).ThenInclude(l => l.Template)
.Include(p => p.CompetitionStats)
    .ThenInclude(cs => cs.Competition)
        .ThenInclude(c => c.Cup).ThenInclude(cu => cu.Template)
.Include(p => p.CompetitionStats)
    .ThenInclude(cs => cs.Competition)
        .ThenInclude(c => c.EuropeanCup).ThenInclude(ec => ec.Template)

                .Where(p => p.Id == playerId)
                .Select(p => new PlayerDto
                {
                    Id = p.Id,
                    FullName = p.FirstName + " " + p.LastName,
                    Position = p.Position.Name,
                    PositionId = p.PositionId,
                    KitNumber = p.KitNumber,
                    Age = p.Age,
                    Country = p.Country != null ? p.Country.Name : "",
                    HeightCm = p.HeightCm,
                    WeightKg = p.WeightKg,
                    Price = p.Price,
                    TeamName = p.Team != null ? p.Team.Name : null,
                    AvatarFileName = p.AvatarFileName,

                    Attributes = p.Attributes.Select(a => new PlayerAttributeDto
                    {
                        AttributeId = a.AttributeId,
                        Name = a.Attribute.Name,
                        Value = a.Value,
                        Category = a.Attribute.Category
                    }).ToList(),

                    SeasonStats = p.SeasonStats.Select(s => new PlayerSeasonStatsDto
                    {
                        SeasonId = s.SeasonId,
                        MatchesPlayed = s.MatchesPlayed,
                        Goals = s.Goals
                    }).ToList(),

                    CompetitionStats = p.CompetitionStats.Select(cs => new PlayerCompetitionStatsDto
                    {
                        CompetitionId = cs.CompetitionId,
                        CompetitionName =
                            cs.Competition.League != null ? cs.Competition.League.Template.Name :
                            cs.Competition.Cup != null ? cs.Competition.Cup.Template.Name :
                            cs.Competition.EuropeanCup != null ? cs.Competition.EuropeanCup.Template.Name :
                            "",
                        MatchesPlayed = cs.MatchesPlayed,
                        Goals = cs.Goals
                    }).ToList()

                })
                .FirstOrDefaultAsync();
        }
        public async Task<ICollection<PlayerDto>> GetPlayersByTeamIdAsync(int teamId)
        {
            return await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Country)
                .Include(p => p.Team)
                .Where(p => p.TeamId == teamId)
                .Select(p => new PlayerDto
                {
                    Id = p.Id,
                    FullName = p.FirstName + " " + p.LastName,
                    Position = p.Position.Name,
                    PositionId = p.PositionId,
                    KitNumber = p.KitNumber,
                    Age = p.Age,
                    Country = p.Country != null ? p.Country.Name : "",
                    HeightCm = p.HeightCm,
                    WeightKg = p.WeightKg,
                    Price = p.Price,
                    TeamName = p.Team != null ? p.Team.Name : null,
                    AvatarFileName = p.AvatarFileName
                })
                .ToListAsync();
        }
        public async Task<ICollection<PlayerAttributeDto>> GetPlayerAttributesAsync(int playerId)
        {
            return await _context.PlayerAttributes
                .Include(a => a.Attribute)
                .Where(a => a.PlayerId == playerId)
                .Select(a => new PlayerAttributeDto
                {
                    AttributeId = a.AttributeId,
                    Name = a.Attribute.Name,
                    Value = a.Value
                })
                .ToListAsync();
        }
        public async Task<ICollection<PlayerSeasonStatsDto>> GetPlayerSeasonStatsAsync(int playerId)
        {
            return await _context.PlayerSeasonStats
                .Where(s => s.PlayerId == playerId)
                .Select(s => new PlayerSeasonStatsDto
                {
                    SeasonId = s.SeasonId,
                    MatchesPlayed = s.MatchesPlayed,
                    Goals = s.Goals
                })
                .ToListAsync();
        }

        public async Task Aging(int gameSaveId)
        {
            var players = await _context.Players
                .Where(p => p.IsActive && p.GameSaveId == gameSaveId)
                .ToListAsync();

            var currentSeason = await _context.Seasons
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            var gameDate = currentSeason?.CurrentDate ?? DateTime.UtcNow;

            foreach (var player in players)
            {
                var age = player.GetAge(gameDate);

                if (age >= 35)
                    await RetirePlayer(player.Id);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RetirePlayer(int playerId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(x => x.Id == playerId);

            if (player == null)
                throw new ArgumentException("Null Player!");

            player.IsActive = false;
            player.TeamId = null;

            await _messageOrchestrator.SendMessageAsync(
        MessageCategory.Retirement,
        player.GameSaveId,
        player
    );

            await _context.SaveChangesAsync();

        }

    }
}
