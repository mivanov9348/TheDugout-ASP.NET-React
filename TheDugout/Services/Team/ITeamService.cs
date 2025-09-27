using TheDugout.Data.DtoGameSave;
using TheDugout.DTOs.Player;

namespace TheDugout.Services.Team
{
    public interface ITeamService
    {
        Task<TeamDto?> GetMyTeamAsync(int userId);
        Task<TeamDto?> GetTeamBySaveAsync(int saveId);

    }
}
