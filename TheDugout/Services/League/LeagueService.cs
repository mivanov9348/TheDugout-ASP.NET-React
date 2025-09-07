using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services.Team;

namespace TheDugout.Services.League
{
    public class LeagueService : ILeagueService
    {
        private readonly DugoutDbContext _context;
        private readonly ITeamGenerationService _teamGenerator;

        public LeagueService(DugoutDbContext context, ITeamGenerationService teamGenerator)
        {
            _context = context;
            _teamGenerator = teamGenerator;
        }

        public async Task<List<Models.League>> GenerateLeaguesAsync(GameSave gameSave)
        {
            var leagues = new List<Models.League>();

            var leagueTemplates = await _context.LeagueTemplates
                .Include(lt => lt.TeamTemplates)
                .ToListAsync();

            foreach (var lt in leagueTemplates)
            {
                var league = new Models.League
                {
                    TemplateId = lt.Id,
                    GameSave = gameSave,
                    CountryId = lt.CountryId,
                    Tier = lt.Tier,
                    TeamsCount = lt.TeamsCount,
                    RelegationSpots = lt.RelegationSpots,
                    PromotionSpots = lt.PromotionSpots
                };

                var teams = _teamGenerator.GenerateTeams(gameSave, league, lt.TeamTemplates);          

                leagues.Add(league);
            }

            return leagues;
        }
    }
}

