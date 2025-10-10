using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Enums;
using TheDugout.Models.Fixtures;
using TheDugout.Services.Season;

namespace TheDugout.Services.EuropeanCup
{
    public class EurocupKnockoutService : IEurocupKnockoutService
    {
        private readonly DugoutDbContext _context;
        private readonly IEurocupScheduleService _eurocupScheduleService;

        public EurocupKnockoutService(DugoutDbContext context, IEurocupScheduleService eurocupScheduleService)
        {
            _context = context;
            _eurocupScheduleService = eurocupScheduleService;
        }

        public async Task DeterminePostGroupAdvancementAsync(int europeanCupId)
        {
            var cup = await _context.EuropeanCups
                .Include(c => c.Standings)
                .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate) // важно: имаме PhaseTemplate
                .Include(c => c.Phases).ThenInclude(p => p.Fixtures)
                .FirstOrDefaultAsync(c => c.Id == europeanCupId)
                ?? throw new InvalidOperationException("Cup not found");

            // league phase (order == 1)
            var leaguePhase = cup.Phases.FirstOrDefault(p => p.PhaseTemplate != null && p.PhaseTemplate.Order == 1);
            if (leaguePhase == null)
                throw new InvalidOperationException("League phase not found");

            // убедим се, че всички мачове са маркирани като Played
            bool allMatchesFinished = leaguePhase.Fixtures != null && leaguePhase.Fixtures.All(f => f.Status == FixtureStatusEnum.Played);
            if (!allMatchesFinished)
                throw new InvalidOperationException("Not all group matches have been played.");

            // standings
            var sorted = cup.Standings.OrderBy(s => s.Ranking).ToList();

            var qualifiedDirect = sorted.Take(8).Select(s => s.TeamId).ToList();
            var playoffTeams = sorted.Skip(8).Take(16).Select(s => s.TeamId).ToList();
            var eliminated = sorted.Skip(24).Select(s => s.TeamId).ToList();

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
                .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate)
                .Include(c => c.Teams).ThenInclude(t => t.Team)
                .Include(c => c.Season).ThenInclude(s => s.Events)
                .FirstOrDefaultAsync(c => c.Id == europeanCupId)
                ?? throw new InvalidOperationException("Cup not found");

            // Най-добре да вземем последната дата на груповата фаза (order == 1), а не общо
            var leaguePhase = cup.Phases.FirstOrDefault(p => p.PhaseTemplate != null && p.PhaseTemplate.Order == 1);
            DateTime lastGroupMatchDate;

            if (leaguePhase != null && leaguePhase.Fixtures != null && leaguePhase.Fixtures.Any())
            {
                lastGroupMatchDate = leaguePhase.Fixtures.Max(f => f.Date);
            }
            else
            {
                // fallback: всички fixtures (ако няма заредени директно в phase)
                lastGroupMatchDate = await _context.Fixtures
                    .Where(f => f.CompetitionType == CompetitionTypeEnum.EuropeanCup
                             && f.SeasonId == cup.SeasonId
                             && f.GameSaveId == cup.GameSaveId)
                    .OrderByDescending(f => f.Date)
                    .Select(f => f.Date)
                    .FirstOrDefaultAsync();

                if (lastGroupMatchDate == default)
                    lastGroupMatchDate = DateTime.Now;
            }

            // Колко knockout фази остават (включително плейоф)
            var remainingPhases = cup.Phases.Count(p => p.PhaseTemplate != null && p.PhaseTemplate.IsKnockout && p.PhaseTemplate.Order >= 2);
            if (remainingPhases <= 0)
                throw new InvalidOperationException("No remaining knockout phases configured.");

            // Вземаме разпределените дати след груповата фаза
            var knockoutDates = _eurocupScheduleService.AssignKnockoutDatesAfter(cup.Season, lastGroupMatchDate, remainingPhases);

            // Вземаме Playoff phase template (order == 2)
            var playoffPhaseTemplate = cup.Phases
                .Select(p => p.PhaseTemplate)
                .FirstOrDefault(pt => pt != null && pt.Order == 2)
                ?? throw new InvalidOperationException("Playoff round template not found");

            // Намираме или създаваме плейоф фазата
            var playoffPhase = cup.Phases.FirstOrDefault(p => p.PhaseTemplateId == playoffPhaseTemplate.Id);
            if (playoffPhase == null)
            {
                playoffPhase = new EuropeanCupPhase
                {
                    EuropeanCupId = cup.Id,
                    PhaseTemplateId = playoffPhaseTemplate.Id
                };
                // добавяме в контекста, за да получи Id при SaveChanges (ако е необходимо)
                _context.EuropeanCupPhases.Add(playoffPhase);
                await _context.SaveChangesAsync();
            }

            // Отборите за плейоф
            var playoffTeams = cup.Teams
                .Where(t => t.CurrentPhaseOrder == 2 && !t.IsEliminated)
                .ToList();

            // Очакваме четен брой (логиката до тук предполага 16). Ако е нечетен — игнорираме последния (без да сриваме)
            if (!playoffTeams.Any())
                return;

            if (playoffTeams.Count % 2 != 0)
            {
                // предпазно: премахваме последния, записваме предупреждение (можеш да логнеш тук)
                playoffTeams.RemoveAt(playoffTeams.Count - 1);
            }

            var random = new Random();
            var shuffled = playoffTeams.OrderBy(_ => random.Next()).ToList();

            // Датата за текущия кръг — първата от списъка
            var thisRoundDate = knockoutDates.FirstOrDefault();
            if (thisRoundDate == default)
                thisRoundDate = lastGroupMatchDate.AddDays(7);

            // Създаваме fixtures
            for (int i = 0; i < shuffled.Count; i += 2)
            {
                var home = shuffled[i];
                var away = shuffled[i + 1];

                _context.Fixtures.Add(new Models.Fixtures.Fixture
                {
                    GameSaveId = cup.GameSaveId,
                    SeasonId = cup.SeasonId,
                    HomeTeamId = home.TeamId,
                    AwayTeamId = away.TeamId,
                    IsElimination = true,
                    CompetitionType = CompetitionTypeEnum.EuropeanCup,
                    EuropeanCupPhaseId = playoffPhase.Id,
                    Status = FixtureStatusEnum.Scheduled,
                    Date = thisRoundDate
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<object> GetKnockoutFixturesAsync(int europeanCupId)
        {
            var knockoutPhases = await _context.EuropeanCupPhases
                .Include(p => p.PhaseTemplate)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.HomeTeam)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.AwayTeam)
                .Include(p => p.Fixtures)
                    .ThenInclude(f => f.Matches)
                        .ThenInclude(m => m.Penalties)
                .Where(p => p.EuropeanCupId == europeanCupId && p.PhaseTemplate.IsKnockout)
                .OrderBy(p => p.PhaseTemplate.Order)
                .ToListAsync();

            return knockoutPhases.Select(p => new
            {
                round = p.Id,
                name = p.PhaseTemplate.Name,
                matches = p.Fixtures.Select(f => new
                {
                    id = f.Id,
                    homeTeam = f.HomeTeam == null ? null : new
                    {
                        id = f.HomeTeam.Id,
                        name = f.HomeTeam.Name,
                        logoFileName = f.HomeTeam.LogoFileName
                    },
                    awayTeam = f.AwayTeam == null ? null : new
                    {
                        id = f.AwayTeam.Id,
                        name = f.AwayTeam.Name,
                        logoFileName = f.AwayTeam.LogoFileName
                    },
                    homeTeamGoals = f.HomeTeamGoals,
                    awayTeamGoals = f.AwayTeamGoals,
                    date = f.Date,
                    status = f.Status,

                    homeTeamPenalties = f.Matches
                        .SelectMany(m => m.Penalties)
                        .Count(p => p.TeamId == f.HomeTeamId && p.IsScored),

                    awayTeamPenalties = f.Matches
                        .SelectMany(m => m.Penalties)
                        .Count(p => p.TeamId == f.AwayTeamId && p.IsScored)
                })
            });
        }

        public async Task GenerateNextKnockoutPhaseAsync(int europeanCupId, int currentOrder)
        {
            var cup = await _context.EuropeanCups
                .Include(c => c.Phases).ThenInclude(p => p.PhaseTemplate)
                .Include(c => c.Teams)
                .Include(c => c.Season).ThenInclude(s => s.Events)
                .FirstOrDefaultAsync(c => c.Id == europeanCupId)
                ?? throw new InvalidOperationException("Cup not found");

            var nextPhaseTemplate = cup.Phases
                .Select(p => p.PhaseTemplate)
                .FirstOrDefault(pt => pt != null && pt.Order == currentOrder + 1);

            if (nextPhaseTemplate == null)
                return;

            var nextPhase = cup.Phases.FirstOrDefault(p => p.PhaseTemplateId == nextPhaseTemplate.Id);
            if (nextPhase == null)
            {
                nextPhase = new EuropeanCupPhase
                {
                    EuropeanCupId = cup.Id,
                    PhaseTemplateId = nextPhaseTemplate.Id
                };
                _context.EuropeanCupPhases.Add(nextPhase);
                await _context.SaveChangesAsync();
            }

            // 1️⃣ Победителите от текущата фаза
            var currentPhase = cup.Phases.FirstOrDefault(x => x.PhaseTemplate != null && x.PhaseTemplate.Order == currentOrder);
            if (currentPhase == null) return;

            var lastFixtures = await _context.Fixtures
                .Where(f => f.EuropeanCupPhaseId == currentPhase.Id)
                .ToListAsync();

            var winners = lastFixtures
                .Where(f => f.WinnerTeamId.HasValue)
                .Select(f => f.WinnerTeamId!.Value)
                .ToList();

            if (nextPhaseTemplate.Order == 3)
            {
                var directQualified = await _context.EuropeanCupTeams
                    .Where(t => t.EuropeanCupId == europeanCupId
                             && !t.IsEliminated
                             && !t.IsPlayoffParticipant
                             && t.CurrentPhaseOrder == 3)
                    .Select(t => t.TeamId ?? -1)
                    .ToListAsync();

                winners.AddRange(directQualified);
            }

            if (!winners.Any()) return;
            if (winners.Count % 2 != 0)
                winners.RemoveAt(winners.Count - 1);

            var random = new Random();
            var shuffled = winners.OrderBy(_ => random.Next()).ToList();

            var remainingPhases = cup.Phases.Count(p => p.PhaseTemplate != null && p.PhaseTemplate.IsKnockout && p.PhaseTemplate.Order > currentOrder);
            if (remainingPhases <= 0) remainingPhases = 1;

            var lastFixtureDate = lastFixtures.Any() ? lastFixtures.Max(f => f.Date) : DateTime.Now;
            var knockoutDates = _eurocupScheduleService.AssignKnockoutDatesAfter(cup.Season, lastFixtureDate, remainingPhases);
            var thisRoundDate = knockoutDates.FirstOrDefault();
            if (thisRoundDate == default) thisRoundDate = lastFixtureDate.AddDays(7);

            for (int i = 0; i < shuffled.Count; i += 2)
            {
                var home = shuffled[i];
                var away = shuffled[i + 1];

                _context.Fixtures.Add(new Models.Fixtures.Fixture
                {
                    GameSaveId = cup.GameSaveId,
                    SeasonId = cup.SeasonId,
                    HomeTeamId = home,
                    AwayTeamId = away,
                    IsElimination = true,
                    CompetitionType = CompetitionTypeEnum.EuropeanCup,
                    EuropeanCupPhaseId = nextPhase.Id,
                    Status = FixtureStatusEnum.Scheduled,
                    Date = thisRoundDate
                });
            }

            await _context.SaveChangesAsync();
        }

    }
}
