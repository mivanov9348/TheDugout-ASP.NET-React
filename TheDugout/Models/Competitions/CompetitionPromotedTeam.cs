namespace TheDugout.Models.Competitions
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Teams;
    public class CompetitionPromotedTeam
    {
        public int Id { get; set; }
        public int CompetitionSeasonResultId { get; set; }
        public CompetitionSeasonResult CompetitionSeasonResult { get; set; } = null!;
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        public int? GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
    }
}
