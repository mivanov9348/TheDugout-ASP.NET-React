namespace TheDugout.Models.Players
{
    using TheDugout.Models.Common;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    public class PlayerSeasonStats
    {
        public int Id { get; set; }
        public int? PlayerId { get; set; }
        public Player? Player { get; set; }
        public int? SeasonId { get; set; }
        public Season? Season { get; set; }
        public int? GameSaveId { get; set; }
        public GameSave? GameSave { get; set; }
        public int MatchesPlayed { get; set; }
        public int Goals { get; set; }
        public int? CompetitionId { get; set; }
        public Competition? Competition { get; set; }
    }
}
