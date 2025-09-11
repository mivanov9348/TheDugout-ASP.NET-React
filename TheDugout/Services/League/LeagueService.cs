using Bogus;
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

        public async Task<List<Models.League>> GenerateLeaguesAsync(GameSave gameSave, Models.Season season)
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
                    Season = season,    
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

        public async Task InitializeStandingsAsync(GameSave gameSave, Models.Season season)
        {
            var standings = new List<LeagueStanding>();

            foreach (var league in gameSave.Leagues)
            {
                foreach (var team in league.Teams)
                {
                    bool exists = await _context.LeagueStandings
                        .AnyAsync(ls => ls.LeagueId == league.Id && ls.TeamId == team.Id && ls.SeasonId == season.Id);

                    if (!exists)
                    {
                        standings.Add(new LeagueStanding
                        {
                            GameSaveId = gameSave.Id,
                            SeasonId = season.Id,
                            LeagueId = league.Id,
                            TeamId = team.Id
                        });
                    }
                }
            }


            await _context.LeagueStandings.AddRangeAsync(standings);
            await _context.SaveChangesAsync();
        }
    }
}
