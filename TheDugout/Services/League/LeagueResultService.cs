namespace TheDugout.Services.League
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Leagues;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Models.Teams;

    public class LeagueResultService : ILeagueResultService
    {
        private readonly DugoutDbContext _context;

        public LeagueResultService(DugoutDbContext context)
        {
            _context = context;
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

                var champion = orderedStandings.First().Team;
                var runnerUp = orderedStandings.Skip(1).FirstOrDefault()?.Team;

                var relegatedTeams = await GetRelegatedTeamsAsync(league, orderedStandings, seasonId);
                var promotedTeams = await GetPromotedTeamsAsync(league, seasonId);
                var europeanQualified = GetEuropeanQualifiedTeams(league, orderedStandings);

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

                foreach (var team in relegatedTeams)
                {
                    result.RelegatedTeams.Add(new CompetitionRelegatedTeam
                    {
                        TeamId = team.Id,
                        GameSaveId = league.GameSaveId
                    });
                }

                foreach (var team in promotedTeams)
                {
                    result.PromotedTeams.Add(new CompetitionPromotedTeam
                    {
                        TeamId = team.Id,
                        GameSaveId = league.GameSaveId
                    });
                }

                foreach (var team in europeanQualified)
                {
                    result.EuropeanQualifiedTeams.Add(new CompetitionEuropeanQualifiedTeam
                    {
                        TeamId = team.Id,
                        GameSaveId = league.GameSaveId
                    });
                }

                results.Add(result);
            }

            await _context.CompetitionSeasonResults.AddRangeAsync(results);
            await _context.SaveChangesAsync();
            return results;
        }

        private async Task<List<Team>> GetRelegatedTeamsAsync(League league, List<LeagueStanding> orderedStandings, int seasonId)
        {
            if (league.RelegationSpots == 0)
                return new List<Team>();

            var lowerLeagueExists = await _context.Leagues
                .AnyAsync(l => l.CountryId == league.CountryId && l.Tier == league.Tier + 1 && l.SeasonId == seasonId);

            if (!lowerLeagueExists)
                return new List<Team>();

            return orderedStandings
                .TakeLast(league.RelegationSpots)
                .Select(s => s.Team)
                .ToList();
        }

        private async Task<List<Team>> GetPromotedTeamsAsync(League league, int seasonId)
        {
            if (league.PromotionSpots == 0)
                return new List<Team>();

            var lowerLeagueStandings = await _context.LeagueStandings
                .Include(s => s.Team)
                .Where(s => s.League.CountryId == league.CountryId &&
                            s.League.Tier == league.Tier + 1 &&
                            s.SeasonId == seasonId)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .Take(league.PromotionSpots)
                .ToListAsync();

            return lowerLeagueStandings.Select(s => s.Team).ToList();
        }

        private List<Team> GetEuropeanQualifiedTeams(League league, List<LeagueStanding> orderedStandings)
        {
            if (league.Tier != 1)
                return new List<Team>();

            return orderedStandings
                .Take(3)
                .Select(s => s.Team)
                .ToList();
        }
    }
}
