namespace TheDugout.Services.League
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Leagues;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Team.Interfaces;

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
                .AsNoTracking()
                .Where(lt => lt.IsActive)
                .ToDictionaryAsync(lt => lt.Id);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var lt in leagueTemplates.Values)
            {
                var competition = new Competition
                {
                    Type = CompetitionTypeEnum.League,
                    GameSaveId = gameSave.Id,
                    SeasonId = season.Id
                };

                leagues.Add(new League
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
                });
            }

            await _context.Leagues.AddRangeAsync(leagues);
            await _context.SaveChangesAsync();

            // ❌ махаме паралелното изпълнение
            // ✅ изпълняваме последователно, за да няма конфликт с DbContext
            foreach (var league in leagues)
            {
                var lt = leagueTemplates[league.TemplateId];
                var teams = await _teamGenerator.GenerateTeamsAsync(gameSave, league, lt.TeamTemplates);
                league.Teams = teams;
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            return leagues;
        }

        public async Task InitializeStandingsAsync(GameSave gameSave, Models.Seasons.Season season)
        {
            var standings = new List<LeagueStanding>();

            var existingStandings = await _context.LeagueStandings
                .Where(ls => ls.SeasonId == season.Id)
                .Select(ls => new { ls.LeagueId, ls.TeamId })
                .ToListAsync();

            var existingSet = new HashSet<(int, int?)>(existingStandings.Select(x => (x.LeagueId, x.TeamId)));

            foreach (var league in gameSave.Leagues)
            {
                var sortedTeams = league.Teams
                    .OrderByDescending(t => t.Popularity)
                    .ThenBy(t => t.Name)
                    .ToList();

                for (int i = 0; i < sortedTeams.Count; i++)
                {
                    var team = sortedTeams[i];
                    if (!existingSet.Contains((league.Id, team.Id)))
                    {
                        standings.Add(new LeagueStanding
                        {
                            GameSaveId = gameSave.Id,
                            SeasonId = season.Id,
                            LeagueId = league.Id,
                            TeamId = team.Id,
                            Ranking = i + 1
                        });
                    }
                }
            }

            await _context.LeagueStandings.AddRangeAsync(standings);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsLeagueFinishedAsync(int leagueId)
        {
            bool allMatchesPlayed = !await _context.Fixtures
                .AnyAsync(f => f.LeagueId == leagueId && f.Status != MatchStageEnum.Played);

            if (allMatchesPlayed)
            {
                var league = await _context.Leagues.FirstOrDefaultAsync(l => l.Id == leagueId);
                if (league != null && !league.IsFinished)
                {
                    league.IsFinished = true;
                    await _context.SaveChangesAsync();
                }
            }
            return allMatchesPlayed;
        }
    }
}
