namespace TheDugout.Services.League
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Leagues;
    using TheDugout.Models.Teams;
    using TheDugout.Services.GameSettings.Interfaces;
    using TheDugout.Services.League.Interfaces;

    public class LeagueResultService : ILeagueResultService
    {
        private readonly DugoutDbContext _context;
        private readonly IMoneyPrizeService _moneyPrizeService;
        private readonly ILogger<LeagueResultService> _logger;
        public LeagueResultService(DugoutDbContext context, IMoneyPrizeService moneyPrizeService, ILogger<LeagueResultService> logger)
        {
            _context = context;
            _moneyPrizeService = moneyPrizeService;
            _logger = logger;
        }

        public async Task<List<CompetitionSeasonResult>> GenerateLeagueResultsAsync(int seasonId)
        {
            bool alreadyExists = await _context.CompetitionSeasonResults
            .AnyAsync(r => r.SeasonId == seasonId && r.CompetitionType == CompetitionTypeEnum.League);

            if (alreadyExists)
                return new List<CompetitionSeasonResult>();

            var gameSave = _context.GameSaves
                    .FirstOrDefault(gs=>gs.CurrentSeasonId == seasonId);

            if (gameSave == null)
            {
                throw new Exception("No Game Save");
            }

            var leagues = await _context.Leagues
                .Include(l => l.Country)
                .Include(l => l.Teams)
                .Include(l => l.Standings)
                .Include(l => l.Template)
                .Include(l=>l.Competition)
                .Where(l => l.SeasonId == seasonId && l.IsFinished)
                .ToListAsync();

            var results = new List<CompetitionSeasonResult>();

            var alreadyQualified = new HashSet<int?>();

            foreach (var league in leagues)
            {
                var orderedStandings = league.Standings
                    .OrderByDescending(s => s.Points)
                    .ThenByDescending(s => s.GoalDifference)
                    .ThenByDescending(s => s.GoalsFor)
                    .ToList();

                if (!orderedStandings.Any())
                    continue;

                var competition = _context.Competitions.FirstOrDefault(x => x.LeagueId == league.Id);

                var champion = orderedStandings.First().Team;
                var runnerUp = orderedStandings.Skip(1).FirstOrDefault()?.Team;

                var relegatedTeams = await GetRelegatedTeamsAsync(league, orderedStandings, seasonId);
                var promotedTeams = await GetPromotedTeamsAsync(league, seasonId);
                var europeanQualified = GetEuropeanQualifiedTeams(league, orderedStandings, alreadyQualified);

                var result = new CompetitionSeasonResult
                {
                    SeasonId = seasonId,
                    CompetitionType = CompetitionTypeEnum.League,
                    CompetitionId = competition.Id,
                    GameSaveId = league.GameSaveId,
                    ChampionTeamId = champion.Id,
                    RunnerUpTeamId = runnerUp?.Id,
                    Notes = $"Лига {league.Template.Name} ({league.Country.Name}) - Ниво {league.Tier}", 
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

                for (int i = 0; i < orderedStandings.Count; i++)
                {
                    var standing = orderedStandings[i];
                    var team = standing.Team;
                    string prizeCode;

                    if (i == 0)
                        prizeCode = "LEAGUE_CHAMPION";
                    else if (i == 1)
                        prizeCode = "LEAGUE_SECOND";
                    else if (i == 2)
                        prizeCode = "LEAGUE_THIRD";
                    else
                        prizeCode = "LEAGUE_SOLIDARITY";

                    await _moneyPrizeService.GrantToTeamAsync(
                        gameSave,
                        prizeCode,
                        team,
                        $"Награда за {i + 1}-во място в {league.Template.Name}"
                    );
                }

                results.Add(result);
            }

            foreach (var r in results)
            {
                _logger.LogInformation("🏆 League '{LeagueName}' -> CompetitionId={CompetitionId}",
                    r.Notes, r.CompetitionId);
            }


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
            if (league.PromotionSpots == 0 || league.Tier == 1)
                return new List<Team>();

            var orderedStandings = await _context.LeagueStandings
                .Include(s => s.Team)
                .Where(s => s.LeagueId == league.Id && s.SeasonId == seasonId)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .Take(league.PromotionSpots)
                .ToListAsync();

            _logger.LogInformation(
                "⬆️ Promotion check for {LeagueName} (Tier {Tier}) found {Count} promoted teams",
                league.Template.Name, league.Tier, orderedStandings.Count
            );

            return orderedStandings.Select(s => s.Team).ToList();
        }


        private List<Team> GetEuropeanQualifiedTeams(League league, List<LeagueStanding> orderedStandings, HashSet<int?> alreadyQualifiedTeamIds)
        {
            if (league.Tier != 1)
                return new List<Team>();

            var qualified = new List<Team>();

            foreach (var standing in orderedStandings)
            {
                if (qualified.Count >= 3)
                    break;

                if (alreadyQualifiedTeamIds.Contains(standing.TeamId))
                    continue; 

                qualified.Add(standing.Team);
                alreadyQualifiedTeamIds.Add(standing.TeamId);
            }

            return qualified;
        }

    }
}
