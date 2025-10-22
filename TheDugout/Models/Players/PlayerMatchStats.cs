namespace TheDugout.Models.Players
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Seasons;

    public class PlayerMatchStats
    {
        public int Id { get; set; }
        public double MatchRating { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;
        public int CompetitionId { get; set; }
        public Competition Competition { get; set; } = null!;
        public int GameSaveId { get; set; }
        public Season Season { get; set; } = null!;
        public int? SeasonId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int Goals { get; set; }
    }
}
