using TheDugout.DTOs.DtoNewGame;

namespace TheDugout.Services.Template
{
    public interface ITemplateService
    {
        Task<List<TeamTemplateDto>> GetTeamTemplatesAsync();
    }
}
