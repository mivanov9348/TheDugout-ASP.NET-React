// Services/PlayerInfoService.cs
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.Player;
using TheDugout.Services.Interfaces;

namespace TheDugout.Services
{
    public class PlayerInfoService : IPlayerInfoService
    {
        private readonly DugoutDbContext _context;

        public PlayerInfoService(DugoutDbContext context)
        {
            _context = context;
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
                    AvatarUrl =  p.AvatarFileName,
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
    }
}
