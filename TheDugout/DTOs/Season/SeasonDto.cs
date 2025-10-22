using TheDugout.Models.Enums;

namespace TheDugout.DTOs.Season
{
    public class SeasonOverviewDto
    {
        public int SeasonId { get; set; }
        public bool AllCompetitionsFinished { get; set; }
        public List<CompetitionResultDto> Competitions { get; set; } = new();
    }

    public class CompetitionResultDto
    {
        public int CompetitionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public CompetitionTypeEnum Type { get; set; }
        public string? ChampionTeam { get; set; }
        public string? RunnerUpTeam { get; set; }
        public List<string> PromotedTeams { get; set; } = new();
        public List<string> RelegatedTeams { get; set; } = new();
        public List<string> EuropeanQualifiedTeams { get; set; } = new();
        public List<AwardDto> Awards { get; set; } = new();
    }

    public class AwardDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public CompetitionAwardType AwardType { get; set; }
        public int Value { get; set; }
    }

}
