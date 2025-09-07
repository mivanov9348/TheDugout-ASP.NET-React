namespace TheDugout.Models
{
    public class Tactic
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int Defenders { get; set; }
        public int Midfielders { get; set; }
        public int Forwards { get; set; }

        public ICollection<TeamTactic> TeamTactics { get; set; } = new List<TeamTactic>();
    }
}
