namespace TheDugout.Models
{
    public class TeamTactic
    {
        public int Id { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int TacticId { get; set; }
        public Tactic Tactic { get; set; } = null!;

        public string CustomName { get; set; } = "Default Tactic";
    }
}
