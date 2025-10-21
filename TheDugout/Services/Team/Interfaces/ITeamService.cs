namespace TheDugout.Services.Team.Interfaces
{
    using TheDugout.DTOs.Team;
    public interface ITeamService
    {
        Task<TeamDto?> GetMyTeamAsync(int userId);
        Task<TeamDto?> GetTeamBySaveAsync(int saveId);
    }
}
