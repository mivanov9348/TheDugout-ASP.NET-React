namespace TheDugout.Models.Competitions
{
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;
    public class CompetitionAward
    {
        public int Id { get; set; }
        public CompetitionAwardType AwardType { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int Value { get; set; }
        public int CompetitionSeasonResultId { get; set; }
        public CompetitionSeasonResult CompetitionSeasonResult { get; set; } = null!;
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int? CompetitionId { get; set; }
        public Competition? Competition { get; set; }
        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;
    }
}
