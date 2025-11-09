using TheDugout.DTOs.DtoNewGame;

namespace TheDugout.Services.Template.Interfaces
{
    public interface ITemplateService
    {
        Task<List<TeamTemplateDto>> GetTeamTemplatesAsync();
    }
}
