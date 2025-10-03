using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Fixtures;

namespace TheDugout.Services.League
{

    public class LeagueStandingsService : ILeagueStandingsService
    {
        private readonly DugoutDbContext _context;

        public LeagueStandingsService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task UpdateStandingsAfterMatchAsync(Models.Fixtures.Fixture fixture)
        {
            // само за лигата
            if (fixture.CompetitionType != CompetitionType.League || fixture.LeagueId == null)
                return;

            var homeStanding = await _context.LeagueStandings
                .FirstOrDefaultAsync(s => s.TeamId == fixture.HomeTeamId && s.LeagueId == fixture.LeagueId);
            var awayStanding = await _context.LeagueStandings
                .FirstOrDefaultAsync(s => s.TeamId == fixture.AwayTeamId && s.LeagueId == fixture.LeagueId);

            if (homeStanding == null || awayStanding == null)
                throw new InvalidOperationException("Standings not initialized for this league");

            int homeGoals = fixture.HomeTeamGoals ?? 0;
            int awayGoals = fixture.AwayTeamGoals ?? 0;

            // 1. Мачове
            homeStanding.Matches++;
            awayStanding.Matches++;

            // 2. Голове
            homeStanding.GoalsFor += homeGoals;
            homeStanding.GoalsAgainst += awayGoals;
            awayStanding.GoalsFor += awayGoals;
            awayStanding.GoalsAgainst += homeGoals;

            homeStanding.GoalDifference = homeStanding.GoalsFor - homeStanding.GoalsAgainst;
            awayStanding.GoalDifference = awayStanding.GoalsFor - awayStanding.GoalsAgainst;

            // 3. Точки / Победи / Загуби / Равни
            if (homeGoals > awayGoals)
            {
                homeStanding.Wins++;
                homeStanding.Points += 3;
                awayStanding.Losses++;
            }
            else if (awayGoals > homeGoals)
            {
                awayStanding.Wins++;
                awayStanding.Points += 3;
                homeStanding.Losses++;
            }
            else
            {
                homeStanding.Draws++;
                awayStanding.Draws++;
                homeStanding.Points++;
                awayStanding.Points++;
            }

            await _context.SaveChangesAsync();

            // 4. Пресмятане на класирането
            var standings = await _context.LeagueStandings
                .Where(s => s.LeagueId == fixture.LeagueId)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .ToListAsync();

            for (int i = 0; i < standings.Count; i++)
                standings[i].Ranking = i + 1;

            await _context.SaveChangesAsync();
        }
    }
}
