
using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;

namespace TheDugout.Services.Standings
{
    public class EuropeanCupStandingService : IEuropeanCupStandingService
    {
        private readonly DugoutDbContext _context;

        public EuropeanCupStandingService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task UpdateEuropeanCupStandingsAfterMatchAsync(int fixtureId, CancellationToken ct = default)
        {
            var fixture = await _context.Set<Models.Fixtures.Fixture>()
                .Include(f => f.EuropeanCupPhase)
                .FirstOrDefaultAsync(f => f.Id == fixtureId, ct)
                ?? throw new InvalidOperationException("Fixture not found");

            if (fixture.HomeTeamGoals == null || fixture.AwayTeamGoals == null)
            {
                return;
            }

            var cupId = fixture.EuropeanCupPhase?.EuropeanCupId
                ?? throw new InvalidOperationException("Fixture is not linked to EuropeanCupPhase");

            var homeStanding = await GetOrCreateStanding(cupId, fixture.HomeTeamId, ct);
            var awayStanding = await GetOrCreateStanding(cupId, fixture.AwayTeamId, ct);

            int homeGoals = fixture.HomeTeamGoals.Value;
            int awayGoals = fixture.AwayTeamGoals.Value;

            homeStanding.Matches++;
            awayStanding.Matches++;

            homeStanding.GoalsFor += homeGoals;
            homeStanding.GoalsAgainst += awayGoals;
            awayStanding.GoalsFor += awayGoals;
            awayStanding.GoalsAgainst += homeGoals;

            homeStanding.GoalDifference = homeStanding.GoalsFor - homeStanding.GoalsAgainst;
            awayStanding.GoalDifference = awayStanding.GoalsFor - awayStanding.GoalsAgainst;

            if (homeGoals > awayGoals)
            {
                homeStanding.Wins++;
                homeStanding.Points += 3;
                awayStanding.Losses++;
            }
            else if (homeGoals < awayGoals)
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

            // --- подреждане по точки, голова разлика и голове ---
            var standings = await _context.Set<EuropeanCupStanding>()
                .Where(s => s.EuropeanCupId == cupId)
                .ToListAsync(ct);

            var ranked = standings
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
                ranked[i].Ranking = i + 1;

            await _context.SaveChangesAsync(ct);

        }

        private async Task<EuropeanCupStanding> GetOrCreateStanding(int cupId, int teamId, CancellationToken ct)
        {
            var standing = await _context.Set<EuropeanCupStanding>()
                .FirstOrDefaultAsync(s => s.EuropeanCupId == cupId && s.TeamId == teamId, ct);

            if (standing == null)
            {
                standing = new EuropeanCupStanding
                {
                    EuropeanCupId = cupId,
                    TeamId = teamId
                };
                _context.Add(standing);
            }

            return standing;
        }
    }
}

