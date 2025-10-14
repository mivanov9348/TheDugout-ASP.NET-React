namespace TheDugout.Services.League
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Services.Team;
    public class LeagueService : ILeagueService
    {
        private readonly DugoutDbContext _context;
        private readonly ITeamGenerationService _teamGenerator;

        public LeagueService(DugoutDbContext context, ITeamGenerationService teamGenerator)
        {
            _context = context;
            _teamGenerator = teamGenerator;
        }
        public async Task<List<League>> GenerateLeaguesAsync(GameSave gameSave, Models.Seasons.Season season)
        {
            var leagues = new List<League>();

            var leagueTemplates = await _context.LeagueTemplates
                .Include(lt => lt.TeamTemplates)
                .AsNoTracking() // няма нужда да се track-ват шаблоните
                .ToListAsync();

            // turn off tracking to improve performance during bulk operations
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var lt in leagueTemplates)
            {
                var competition = new Competition
                {
                    Type = CompetitionTypeEnum.League,
                    GameSaveId = gameSave.Id,
                    SeasonId = season.Id
                };

                var league = new League
                {
                    TemplateId = lt.Id,
                    GameSaveId = gameSave.Id,
                    Season = season,
                    CountryId = lt.CountryId,
                    Tier = lt.Tier,
                    TeamsCount = lt.TeamsCount,
                    RelegationSpots = lt.RelegationSpots,
                    PromotionSpots = lt.PromotionSpots,
                    Competition = competition
                };

                leagues.Add(league);
            }

            _context.Leagues.AddRange(leagues);
            await _context.SaveChangesAsync();

            // generate teams for each league
            foreach (var league in leagues)
            {
                var lt = leagueTemplates.First(x => x.Id == league.TemplateId);
                var teams = await _teamGenerator.GenerateTeamsAsync(gameSave, league, lt.TeamTemplates);
                league.Teams = teams;
            }

            // turn tracking back on
            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return leagues;
        }


        public async Task InitializeStandingsAsync(GameSave gameSave, Models.Seasons.Season season)
        {
            var standings = new List<LeagueStanding>();

            foreach (var league in gameSave.Leagues)
            {

                var teamsInLeague = league.Teams
                    .Select(t => new { Team = t, LeagueId = league.Id })
                    .ToList();

                var sortedTeams = teamsInLeague
                    .OrderByDescending(x => x.Team.Popularity)
                    .ThenBy(x => x.Team.Name)
                    .ToList();

                for (int i = 0; i < sortedTeams.Count; i++)
                {
                    var team = sortedTeams[i].Team;
                    var leagueId = sortedTeams[i].LeagueId;

                    bool exists = await _context.LeagueStandings
                        .AnyAsync(ls => ls.LeagueId == leagueId && ls.TeamId == team.Id && ls.SeasonId == season.Id);

                    if (!exists)
                    {
                        standings.Add(new LeagueStanding
                        {
                            GameSaveId = gameSave.Id,
                            SeasonId = season.Id,
                            LeagueId = leagueId,
                            TeamId = team.Id,
                            Ranking = i + 1
                        });
                    }
                }
            }

            await _context.LeagueStandings.AddRangeAsync(standings);
            await _context.SaveChangesAsync();
        }
    }
}
