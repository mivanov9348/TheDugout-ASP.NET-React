// Services/Interfaces/IPlayerInfoService.cs
using TheDugout.DTOs.Player;

namespace TheDugout.Services.Interfaces
{
    public interface IPlayerInfoService
    {
        Task<PlayerDto?> GetPlayerByIdAsync(int playerId);
        Task<ICollection<PlayerDto>> GetPlayersByTeamIdAsync(int teamId);

        Task<ICollection<PlayerAttributeDto>> GetPlayerAttributesAsync(int playerId);
        Task<ICollection<PlayerSeasonStatsDto>> GetPlayerSeasonStatsAsync(int playerId);
    }
}
