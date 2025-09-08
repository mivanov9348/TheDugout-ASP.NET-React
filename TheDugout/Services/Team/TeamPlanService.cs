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

        public async Task InitializeDefaultTacticsAsync(GameSave gameSave)
        {
            var teams = await _context.Teams
                .Include(t => t.Players)
                    .ThenInclude(p => p.Position)
                .Where(t => t.GameSaveId == gameSave.Id)
                .ToListAsync();

            var defaultTactic = await _context.Tactics.FirstOrDefaultAsync(t =>
                t.Defenders == 4 && t.Midfielders == 4 && t.Forwards == 2);

            if (defaultTactic == null)
                throw new Exception("Тактиката 4-4-2 липсва в базата.");

            foreach (var team in teams)
            {
                if (await _context.TeamTactics.AnyAsync(tt => tt.TeamId == team.Id))
                    continue;

                var lineup = new Dictionary<string, string?>();
                var players = team.Players.ToList();

                var gk = players.Where(p => p.Position.Code == "GK").Take(1).ToList();
                var df = players.Where(p => p.Position.Code == "DF").Take(4).ToList();
                var mid = players.Where(p => p.Position.Code == "MID").Take(4).ToList();
                var att = players.Where(p => p.Position.Code == "ATT").Take(2).ToList();

                int index = 1;
                foreach (var p in gk) lineup.Add($"GK{index++}", p.Id.ToString());
                index = 1;
                foreach (var p in df) lineup.Add($"DF{index++}", p.Id.ToString());
                index = 1;
                foreach (var p in mid) lineup.Add($"MID{index++}", p.Id.ToString());
                index = 1;
                foreach (var p in att) lineup.Add($"ATT{index++}", p.Id.ToString());

                var tactic = new TeamTactic
                {
                    TeamId = team.Id,
                    TacticId = defaultTactic.Id,
                    CustomName = "Default 4-4-2",
                    LineupJson = System.Text.Json.JsonSerializer.Serialize(lineup)
                };

                _context.TeamTactics.Add(tactic);
            }

            await _context.SaveChangesAsync();
        }

    }
}