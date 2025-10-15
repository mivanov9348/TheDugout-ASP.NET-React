using TheDugout.DTOs.DtoGameSave;
using TheDugout.DTOs.Player;

namespace TheDugout.Services.Team.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDto?> GetMyTeamAsync(int userId);
        Task<TeamDto?> GetTeamBySaveAsync(int saveId);

    }
}
