namespace TheDugout.Models.Game
{
    using TheDugout.Models.Players;
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int? CurrentSaveId { get; set; }
        public GameSave? CurrentSave { get; set; }
        public bool IsAdmin { get; set; } = false;
        public ICollection<GameSave> GameSaves { get; set; } = new List<GameSave>();

        public ICollection<Shortlist> Shortlist { get; set; } = new List<Shortlist>();

    }
}
