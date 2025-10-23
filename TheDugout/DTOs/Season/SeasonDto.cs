namespace TheDugout.DTOs.Season
{
    using TheDugout.Models.Enums;

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

        public TeamSummaryDto? ChampionTeam { get; set; }
        public TeamSummaryDto? RunnerUpTeam { get; set; }

        public List<TeamSummaryDto> PromotedTeams { get; set; } = new();
        public List<TeamSummaryDto> RelegatedTeams { get; set; } = new();
        public List<TeamSummaryDto> EuropeanQualifiedTeams { get; set; } = new();

        public List<AwardDto> Awards { get; set; } = new();
        public List<LeagueStandingDto>? LeagueStandings { get; set; }
        public List<TopScorerDto>? TopScorers { get; set; }
    }

    public class AwardDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public CompetitionAwardType AwardType { get; set; }
        public int Value { get; set; }
    }

    public class LeagueStandingDto
    {
        public string TeamName { get; set; } = string.Empty;
        public string? TeamLogo { get; set; } 
        public int Points { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
    }

    public class TopScorerDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public int Goals { get; set; }
    }

    public class TeamSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoFileName { get; set; }
    }

}
