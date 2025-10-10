namespace TheDugout.Models.Teams
{
    public class TeamTactic
    {
        public int Id { get; set; }

        public int? TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int? TacticId { get; set; }
        public Tactic Tactic { get; set; } = null!;
        public string LineupJson { get; set; } = "{}";
        public string SubstitutesJson { get; set; } = "[]";

        public string CustomName { get; set; } = "Default Tactic";
    }
}
