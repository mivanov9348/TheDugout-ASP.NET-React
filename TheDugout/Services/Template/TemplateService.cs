using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.DTOs.DtoNewGame;

namespace TheDugout.Services.Template
{
    public class TemplateService : ITemplateService
    {
        private readonly DugoutDbContext _context;

        public TemplateService(DugoutDbContext context)
        {
            _context = context;
        }
        public async Task<List<TeamTemplateDto>> GetTeamTemplatesAsync()
        {
            return await _context.TeamTemplates
                .AsNoTracking()
                .Include(t => t.League)
                .Select(t => new TeamTemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Abbreviation = t.Abbreviation,
                    CountryId = t.CountryId,
                    LeagueId = t.LeagueId,
                    LeagueName = t.League.Name,
                    Tier = t.League.Tier
                })
                .ToListAsync();
        }
    }
}
