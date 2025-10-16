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
        public async Task<bool> IsLeagueFinishedAsync(int leagueId)
        {
            // Load the league with its fixtures
            var league = await _context.Leagues
                .Include(l => l.Fixtures)
                .FirstOrDefaultAsync(l => l.Id == leagueId);

            if (league == null)
                throw new Exception("League not found");

            // Check if all non-cancelled matches are played
            bool allMatchesPlayed = league.Fixtures
                .Where(f => f.Status != FixtureStatusEnum.Cancelled)
                .All(f => f.Status == FixtureStatusEnum.Played);

            // If all matches are played and the league is not marked as finished, update it
            if (allMatchesPlayed && !league.IsFinished)
            {
                league.IsFinished = true;  // mark the league as finished
                await _context.SaveChangesAsync(); // Save changes to the database
            }

            return allMatchesPlayed;
        }
        public async Task<List<CompetitionSeasonResult>> GenerateLeagueResultsAsync(int seasonId)
        {
            var leagues = await _context.Leagues
                .Include(l => l.Country)
                .Include(l => l.Teams)
                .Include(l => l.Standings)
                .Include(l => l.Template)
                .Where(l => l.SeasonId == seasonId && l.IsFinished)
                .ToListAsync();

            var results = new List<CompetitionSeasonResult>();

            foreach (var league in leagues)
            {
                var orderedStandings = league.Standings
                    .OrderByDescending(s => s.Points)
                    .ThenByDescending(s => s.GoalDifference)
                    .ThenByDescending(s => s.GoalsFor)
                    .ToList();

                if (!orderedStandings.Any())
                    continue;

                // Chamion and Runner-Up
                var champion = orderedStandings.First().Team;
                var runnerUp = orderedStandings.Skip(1).FirstOrDefault()?.Team;

                // Relegated Teams
                var lowerLeague = await _context.Leagues
                    .Where(l => l.CountryId == league.CountryId && l.Tier == league.Tier + 1 && l.SeasonId == seasonId)
                    .FirstOrDefaultAsync();

                var relegatedTeams = new List<Models.Teams.Team>();
                if (lowerLeague != null && league.RelegationSpots > 0)
                {
                    relegatedTeams = orderedStandings
                        .TakeLast(league.RelegationSpots)
                        .Select(s => s.Team)
                        .ToList();
                }

                // Promoted Teams
                var upperLeague = await _context.Leagues
                    .Where(l => l.CountryId == league.CountryId && l.Tier == league.Tier - 1 && l.SeasonId == seasonId)
                    .FirstOrDefaultAsync();

                var promotedTeams = new List<Models.Teams.Team>();
                if (upperLeague != null && league.PromotionSpots > 0)
                {
                    // Get top teams from the lower league
                    var lowerLeagueStandings = await _context.LeagueStandings
                        .Include(s => s.Team)
                        .Where(s => s.League.CountryId == league.CountryId && s.League.Tier == league.Tier + 1 && s.SeasonId == seasonId)
                        .OrderByDescending(s => s.Points)
                        .ThenByDescending(s => s.GoalDifference)
                        .ThenByDescending(s => s.GoalsFor)
                        .Take(league.PromotionSpots)
                        .ToListAsync();

                    promotedTeams = lowerLeagueStandings.Select(s => s.Team).ToList();
                }

                // European Qualified Teams
                // Only for Tier 1 leagues
                var europeanQualified = new List<Models.Teams.Team>();
                if (league.Tier == 1)
                {
                    europeanQualified = orderedStandings
                        .Take(3)
                        .Select(s => s.Team)
                        .ToList();
                }

                // Create CompetitionSeasonResult
                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.League,
                    CompetitionId = league.CompetitionId,
                    GameSaveId = league.GameSaveId,
                    ChampionTeamId = champion.Id,
                    RunnerUpTeamId = runnerUp?.Id,
                    Notes = $"Лига {league.Template.Name} ({league.Country.Name}) - Ниво {league.Tier}"
                };

                foreach (var relegated in relegatedTeams)
                {
                    result.RelegatedTeams.Add(new CompetitionRelegatedTeam
                    {
                        TeamId = relegated.Id,
                        GameSaveId = league.GameSaveId
                    });
                }

                foreach (var promoted in promotedTeams)
                {
                    result.PromotedTeams.Add(new CompetitionPromotedTeam
                    {
                        TeamId = promoted.Id,
                        GameSaveId = league.GameSaveId
                    });
                }

                foreach (var euro in europeanQualified)
                {
                    result.EuropeanQualifiedTeams.Add(new CompetitionEuropeanQualifiedTeam
                    {
                        TeamId = euro.Id,
                        GameSaveId = league.GameSaveId
                    });
                }

                results.Add(result);
            }

            await _context.CompetitionSeasonResults.AddRangeAsync(results);
            await _context.SaveChangesAsync();

            return results;
        }
    }
}
