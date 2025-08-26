namespace TheDugout.Models
{
    public class GameSave
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = "New Save";

        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}
