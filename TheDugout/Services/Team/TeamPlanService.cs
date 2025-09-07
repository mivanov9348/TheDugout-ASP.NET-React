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

        public async Task<TeamTactic> SetTeamTacticAsync(int teamId, int tacticId, string? customName = null)
        {
            var team = await _context.Teams
                .Include(t => t.TeamTactic)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                throw new Exception("Team not found");

            if (team.TeamTactic != null)
            {
                team.TeamTactic.TacticId = tacticId;
                team.TeamTactic.CustomName = customName ?? team.TeamTactic.CustomName;
            }
            else
            {
                var newTactic = new TeamTactic
                {
                    TeamId = teamId,
                    TacticId = tacticId,
                    CustomName = customName ?? "Default Tactic"
                };
                _context.TeamTactics.Add(newTactic);
                team.TeamTactic = newTactic;
            }

            await _context.SaveChangesAsync();

            return team.TeamTactic!;
        }

    }
}
