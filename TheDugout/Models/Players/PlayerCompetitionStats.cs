namespace TheDugout.Models.Players
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    public class PlayerCompetitionStats
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int CompetitionId { get; set; }
        public Competition Competition { get; set; } = null!;

        public int? SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int MatchesPlayed { get; set; }
        public int Goals { get; set; }
    }

}
