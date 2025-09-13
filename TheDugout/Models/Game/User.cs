namespace TheDugout.Models.Game
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public int? CurrentSaveId { get; set; }
        public GameSave? CurrentSave { get; set; }

        public ICollection<GameSave> GameSaves { get; set; } = new List<GameSave>();
    }
}
