using Bogus;
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Common;
using TheDugout.Models.Cups;
using TheDugout.Models.Enums;
using TheDugout.Models.Game;
using TheDugout.Models.Leagues;
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
        public async Task<List<Models.Leagues.League>> GenerateLeaguesAsync(GameSave gameSave, Models.Seasons.Season season)
        {
            var leagues = new List<Models.Leagues.League>();

            var leagueTemplates = await _context.LeagueTemplates
                .Include(lt => lt.TeamTemplates)
                .ToListAsync();

            foreach (var lt in leagueTemplates)
            {
                // 🏆 1. Създаваме Competition
                var competition = new Competition
                {
                    Type = CompetitionTypeEnum.League,
                    SeasonId = season.Id
                };

                _context.Competitions.Add(competition);
                await _context.SaveChangesAsync();

                // 🏆 2. Създаваме League
                var league = new Models.Leagues.League
                {
                    TemplateId = lt.Id,
                    GameSave = gameSave,
                    Season = season,
                    CountryId = lt.CountryId,
                    Tier = lt.Tier,
                    TeamsCount = lt.TeamsCount,
                    RelegationSpots = lt.RelegationSpots,
                    PromotionSpots = lt.PromotionSpots,
                    CompetitionId = competition.Id,
                    Competition = competition,
                };

                competition.League = league;

                _context.Leagues.Add(league);
                await _context.SaveChangesAsync(); // 💾 ID-то вече е налично тук

                // 🏟️ Генерираме отборите
                var teams = await _teamGenerator.GenerateTeamsAsync(gameSave, league, lt.TeamTemplates);
                league.Teams = teams;

                // ✅ Тук не я добавяй пак в контекста!
                // _context.Leagues.Add(league); // ❌ махни този ред

                await _context.SaveChangesAsync(); // обновява само промените по връзките
                leagues.Add(league);
            }

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
