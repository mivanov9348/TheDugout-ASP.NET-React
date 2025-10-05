using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Fixtures;

namespace TheDugout.Services.EuropeanCup
{
    public class EurocupKnockoutService : IEurocupKnockoutService
    {
        private readonly DugoutDbContext _context;

        public EurocupKnockoutService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task DeterminePostGroupAdvancementAsync(int europeanCupId)
        {
            var cup = await _context.EuropeanCups
                .Include(c => c.Standings)
                .Include(c => c.Phases)
                    .ThenInclude(p => p.Fixtures)
                .FirstOrDefaultAsync(c => c.Id == europeanCupId)
                ?? throw new InvalidOperationException("Cup not found");

            // 1️⃣ Проверка: дали всички мачове от груповата фаза са завършили
            var leaguePhase = cup.Phases
                .FirstOrDefault(p => p.PhaseTemplate.Order == 1);

            if (leaguePhase == null)
                throw new InvalidOperationException("League phase not found");

            bool allMatchesFinished = leaguePhase.Fixtures.All(f => f.Status == FixtureStatus.Played);
            if (!allMatchesFinished)
                throw new InvalidOperationException("Not all group matches have been played.");


            // 2️⃣ Сортираме standings по ранкинг
            var sorted = cup.Standings.OrderBy(s => s.Ranking).ToList();

            // 3️⃣ Определяме кой къде отива
            var qualifiedDirect = sorted.Take(8).Select(s => s.TeamId).ToList();
            var playoffTeams = sorted.Skip(8).Take(16).Select(s => s.TeamId).ToList();
            var eliminated = sorted.Skip(24).Select(s => s.TeamId).ToList();

            // 4️⃣ Записваме промените в EuropeanCupTeam
            var teams = await _context.EuropeanCupTeams
                .Where(t => t.EuropeanCupId == europeanCupId)
                .ToListAsync();

            foreach (var team in teams)
            {
                if (qualifiedDirect.Contains(team.TeamId))
                {
                    team.IsEliminated = false;
                    team.IsPlayoffParticipant = false;
                    team.CurrentPhaseOrder = 3; 
                }
                else if (playoffTeams.Contains(team.TeamId))
                {
                    team.IsEliminated = false;
                    team.IsPlayoffParticipant = true;
                    team.CurrentPhaseOrder = 2; 
                }
                else if (eliminated.Contains(team.TeamId))
                {
                    team.IsEliminated = true;
                    team.IsPlayoffParticipant = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task GeneratePlayoffRoundAsync(int europeanCupId)
        {
            var cup = await _context.EuropeanCups
                .Include(c => c.Phases)
                    .ThenInclude(p => p.PhaseTemplate)
                .Include(c => c.Teams)
                    .ThenInclude(t => t.Team)
                .FirstOrDefaultAsync(c => c.Id == europeanCupId)
                ?? throw new InvalidOperationException("Cup not found");

            var playoffPhaseTemplate = cup.Phases
                .Select(p => p.PhaseTemplate)
                .FirstOrDefault(pt => pt.Order == 2)
                ?? throw new InvalidOperationException("Playoff round template not found");

            var playoffPhase = cup.Phases
                .FirstOrDefault(p => p.PhaseTemplateId == playoffPhaseTemplate.Id)
                ?? new EuropeanCupPhase
                {
                    EuropeanCupId = cup.Id,
                    PhaseTemplateId = playoffPhaseTemplate.Id
                };

            // Намираме всички отбори за плейофа
            var playoffTeams = cup.Teams
                .Where(t => t.CurrentPhaseOrder == 2 && !t.IsEliminated)
                .ToList();

            // 16 отбора → 8 двойки
            var random = new Random();
            var shuffled = playoffTeams.OrderBy(_ => random.Next()).ToList();

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                var home = shuffled[i];
                var away = shuffled[i + 1];

                _context.Fixtures.Add(new Models.Fixtures.Fixture
                {
                    HomeTeamId = home.TeamId,
                    AwayTeamId = away.TeamId,
                    CompetitionType = CompetitionType.EuropeanCup,
                    EuropeanCupPhaseId = playoffPhase.Id,
                    Status = FixtureStatus.Scheduled,
                    Date = DateTime.Now.AddDays(7)
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
