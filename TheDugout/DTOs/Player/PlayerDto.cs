namespace TheDugout.DTOs.Player
{
    using TheDugout.Models.Players;
    public class PlayerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Position { get; set; } = "";
        public int? PositionId { get; set; }
        public int KitNumber { get; set; }
        public int Age { get; set; }
        public string Country { get; set; } = "";
        public double? HeightCm { get; set; }
        public double? WeightKg { get; set; }
        public decimal Price { get; set; }
        public string? TeamName { get; set; }
        public string? AvatarFileName { get; set; }
        public List<PlayerAttributeDto> Attributes { get; set; } = new();
        public List<PlayerSeasonStatsDto> SeasonStats { get; set; } = new();
        public List<PlayerCompetitionStatsDto> CompetitionStats { get; set; } = new();
    }
    public class PlayerAttributeDto
    {
        public int? AttributeId { get; set; }
        public string Name { get; set; } = null!;
        public int? Value { get; set; }
        public AttributeCategory Category { get; set; }
    }
    public class PlayerSeasonStatsDto
    {
        public int? SeasonId { get; set; }
        public int? MatchesPlayed { get; set; }
        public int? Goals { get; set; }
        public int? Assists { get; set; }
        public double SeasonRating { get; set; }
    }
    public class PlayerStatsDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public int? Goals { get; set; }
        public int? Matches { get; set; }
    }
    public class PlayerCompetitionStatsDto
    {
        public int CompetitionId { get; set; }
        public string CompetitionName { get; set; } = "";
        public int MatchesPlayed { get; set; }
        public int Goals { get; set; }
    }

}
