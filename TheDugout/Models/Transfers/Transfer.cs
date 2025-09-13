using TheDugout.Models.Game;
using TheDugout.Models.Players;
using TheDugout.Models.Seasons;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Transfers
{
    public class Transfer
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int? FromTeamId { get; set; }
        public Team? FromTeam { get; set; }

        public int ToTeamId { get; set; }
        public Team ToTeam { get; set; } = null!;

        public decimal Fee { get; set; }
        public bool IsFreeAgent { get; set; }

        public DateTime GameDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
