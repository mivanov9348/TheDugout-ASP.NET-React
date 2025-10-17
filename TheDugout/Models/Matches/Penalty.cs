namespace TheDugout.Models.Matches
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Teams;
    public class Penalty
    {
        public int Id { get; set; }

        public int? MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public int? TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int? PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int Order { get; set; }
        public bool IsScored { get; set; }
    }

}
