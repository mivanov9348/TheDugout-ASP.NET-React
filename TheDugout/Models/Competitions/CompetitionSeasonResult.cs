namespace TheDugout.Models.Competitions
{
    using TheDugout.Models.Enums;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Teams;
    public class CompetitionSeasonResult
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public int? CompetitionId { get; set; }
        public Competition? Competition { get; set; }
        public int? GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public CompetitionTypeEnum CompetitionType { get; set; }
        public int? ChampionTeamId { get; set; }
        public Team? ChampionTeam { get; set; }
        public int? RunnerUpTeamId { get; set; }
        public Team? RunnerUpTeam { get; set; }
        public ICollection<CompetitionRelegatedTeam> RelegatedTeams { get; set; } = new List<CompetitionRelegatedTeam>();
        public ICollection<CompetitionPromotedTeam> PromotedTeams { get; set; } = new List<CompetitionPromotedTeam>();
        public ICollection<CompetitionEuropeanQualifiedTeam> EuropeanQualifiedTeams { get; set; } = new List<CompetitionEuropeanQualifiedTeam>();
        public string? Notes { get; set; }
    }
}
