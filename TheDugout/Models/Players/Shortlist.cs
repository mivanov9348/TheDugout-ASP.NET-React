namespace TheDugout.Models.Players
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Teams;
    public class Shortlist
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int? TeamId { get; set; }
        public Team? Team { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }

        public int Priority { get; set; } = 0;
    }
}
