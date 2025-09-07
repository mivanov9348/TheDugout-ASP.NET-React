namespace TheDugout.Models
{
    public class GameSave
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = "New Save";

        public int? UserTeamId { get; set; }
        public Team? UserTeam { get; set; }

        public int BankId { get; set; }
        public Bank Bank { get; set; } = null!;

        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<Season> Seasons { get; set; } = new List<Season>();
        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
    }
}
