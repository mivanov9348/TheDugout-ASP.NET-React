using TheDugout.Data.DtoNewGame;

namespace TheDugout.Services.Template
{
    public interface ITemplateService
    {
        Task<List<TeamTemplateDto>> GetTeamTemplatesAsync();
    }
}
