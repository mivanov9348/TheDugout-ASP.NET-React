namespace TheDugout.Services.EuropeanCup
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Teams;
    using TheDugout.Services.EuropeanCup.Interfaces;
    public class EuroCupTeamService : IEuroCupTeamService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<EuroCupTeamService> _logger;

        public EuroCupTeamService(DugoutDbContext context, ILogger<EuroCupTeamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<EuropeanCupTeam>> CreateTeamsForCupAsync(EuropeanCup euroCup, List<Team> chosenTeams, CancellationToken ct = default)
        {
            var phaseOrder = euroCup.Template?.PhaseTemplates?.OrderBy(p => p.Order).FirstOrDefault()?.Order ?? 1;

            // 🧹 1️⃣ Премахни дубликатни отбори в паметта (ако се повтарят в chosenTeams)
            chosenTeams = chosenTeams
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .ToList();

            // 🧠 2️⃣ Извлечи кои отбори вече са в тази купа
            var existingTeamIds = await _context.EuropeanCupTeams
                .Where(t => t.EuropeanCupId == euroCup.Id)
                .Select(t => t.TeamId)
                .ToListAsync(ct);

            // 🚫 3️⃣ Изключи тези, които вече присъстват
            var newTeams = chosenTeams
                .Where(t => !existingTeamIds.Contains(t.Id))
                .ToList();

            if (!newTeams.Any())
            {
                _logger.LogWarning("No new teams to add for cup {CupId}.", euroCup.Id);
                return new List<EuropeanCupTeam>();
            }

            // ✅ 4️⃣ Добави само уникалните
            var cupTeams = newTeams.Select(team => new EuropeanCupTeam
            {
                EuropeanCupId = euroCup.Id,
                TeamId = team.Id,
                GameSaveId = euroCup.GameSaveId,
                CurrentPhaseOrder = phaseOrder,
                IsEliminated = false,
                IsPlayoffParticipant = false
            }).ToList();

            await _context.EuropeanCupTeams.AddRangeAsync(cupTeams, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Created {Count} Eurocup teams for cup {CupId}", cupTeams.Count, euroCup.Id);
            return cupTeams;
        }


        public async Task MarkTeamEliminatedAsync(int europeanCupId, int teamId, CancellationToken ct = default)
        {
            var team = await _context.EuropeanCupTeams
                .FirstOrDefaultAsync(t => t.EuropeanCupId == europeanCupId && t.TeamId == teamId, ct);

            if (team == null)
            {
                _logger.LogWarning("Tried to eliminate non-existent team {TeamId} in cup {CupId}", teamId, europeanCupId);
                return;
            }

            team.IsEliminated = true;
            _context.Update(team);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Team {TeamId} marked as eliminated in cup {CupId}", teamId, europeanCupId);
        }

        public async Task MarkTeamsEliminatedAsync(int europeanCupId, List<int> teamIds, CancellationToken ct = default)
        {
            var teams = await _context.EuropeanCupTeams
                .Where(t => t.EuropeanCupId == europeanCupId && teamIds.Contains(t.TeamId ?? -1))
                .ToListAsync(ct);

            foreach (var t in teams)
                t.IsEliminated = true;

            _context.UpdateRange(teams);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Marked {Count} teams as eliminated in cup {CupId}", teams.Count, europeanCupId);
        }

        public async Task<List<EuropeanCupTeam>> GetActiveTeamsAsync(int europeanCupId, CancellationToken ct = default)
        {
            return await _context.EuropeanCupTeams
                .Where(t => t.EuropeanCupId == europeanCupId && !t.IsEliminated)
                .ToListAsync(ct);
        }
    }

}
