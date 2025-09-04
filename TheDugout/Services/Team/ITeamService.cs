using TheDugout.Data.DtoGameSave;

namespace TheDugout.Services.Team
{
    public interface ITeamService
    {
        Task<TeamDto?> GetMyTeamAsync(int userId);
        Task<TeamDto?> GetTeamBySaveAsync(int saveId);
    }
}
