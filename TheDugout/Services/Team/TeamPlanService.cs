using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;

namespace TheDugout.Services.Team
{
    public class TeamPlanService : ITeamPlanService
    {
        private readonly DugoutDbContext _context;

        public TeamPlanService(DugoutDbContext context)
        {
            _context = context;
        }

        public Dictionary<string, int> GetDefaultRosterPlan()
        {
            return new Dictionary<string, int>
            {
                { "GK", 1 },
                { "DF", 4 },
                { "MID", 4 },
                { "ATT", 2 },
                { "ANY", 5 }
            };
        }

        public async Task<TeamTactic?> GetTeamTacticAsync(int teamId, int gameSaveId)
        {
            return await _context.TeamTactics
                .Include(tt => tt.Tactic)
                .FirstOrDefaultAsync(tt => tt.TeamId == teamId && tt.Team.GameSaveId == gameSaveId);
        }

        public async Task<TeamTactic> SetTeamTacticAsync(
            int teamId,
            int tacticId,
            string? customName,
            Dictionary<string, string?> lineup)
        {
            var team = await _context.Teams
                .Include(t => t.TeamTactic)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                throw new Exception("Team not found");

            var tactic = await _context.Tactics.FindAsync(tacticId);
            if (tactic == null)
                throw new Exception("Тактиката не съществува");

            int gk = lineup.Count(x => x.Key.StartsWith("GK") && !string.IsNullOrEmpty(x.Value));
            int def = lineup.Count(x => x.Key.StartsWith("DF") && !string.IsNullOrEmpty(x.Value));
            int mid = lineup.Count(x => x.Key.StartsWith("MID") && !string.IsNullOrEmpty(x.Value));
            int att = lineup.Count(x => x.Key.StartsWith("ATT") && !string.IsNullOrEmpty(x.Value));

            if (gk < 1)
                throw new InvalidOperationException("Липсва вратар.");
            if (def < tactic.Defenders)
                throw new InvalidOperationException($"Липсват защитници ({def}/{tactic.Defenders}).");
            if (mid < tactic.Midfielders)
                throw new InvalidOperationException($"Липсват халфове ({mid}/{tactic.Midfielders}).");
            if (att < tactic.Forwards)
                throw new InvalidOperationException($"Липсват нападатели ({att}/{tactic.Forwards}).");

            // Ако има TeamTactic → ъпдейтваме
            if (team.TeamTactic != null)
            {
                team.TeamTactic.TacticId = tacticId;
                team.TeamTactic.CustomName = customName ?? team.TeamTactic.CustomName;
                team.TeamTactic.LineupJson = System.Text.Json.JsonSerializer.Serialize(lineup);
            }
            else
            {
                var newTactic = new TeamTactic
                {
                    TeamId = teamId,
                    TacticId = tacticId,
                    CustomName = customName ?? "Default Tactic",
                    LineupJson = System.Text.Json.JsonSerializer.Serialize(lineup)
                };
                _context.TeamTactics.Add(newTactic);
                team.TeamTactic = newTactic;
            }

            await _context.SaveChangesAsync();
            return team.TeamTactic!;
        }
    }
}
