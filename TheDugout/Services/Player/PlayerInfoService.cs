// Services/PlayerInfoService.cs
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.Player;
using TheDugout.Services.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace TheDugout.Services
{
    public class PlayerInfoService : IPlayerInfoService
    {
        private readonly DugoutDbContext _context;
        private readonly IMapper _mapper;

        public PlayerInfoService(DugoutDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PlayerDto?> GetPlayerByIdAsync(int playerId)
        {
            return await _context.Players
                .Where(p => p.Id == playerId)
                .ProjectTo<PlayerDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<PlayerDto>> GetPlayersByTeamIdAsync(int teamId)
        {
            return await _context.Players
                .Where(p => p.TeamId == teamId)
                .ProjectTo<PlayerDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ICollection<PlayerAttributeDto>> GetPlayerAttributesAsync(int playerId)
        {
            return await _context.PlayerAttributes
                .Where(a => a.PlayerId == playerId)
                .ProjectTo<PlayerAttributeDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ICollection<PlayerSeasonStatsDto>> GetPlayerSeasonStatsAsync(int playerId)
        {
            return await _context.PlayerSeasonStats
                .Where(s => s.PlayerId == playerId)
                .ProjectTo<PlayerSeasonStatsDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}
