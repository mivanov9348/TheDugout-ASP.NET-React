using TheDugout.Models.Players;

namespace TheDugout.DTOs.Player
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Position { get; set; } = null!;
        public int? PositionId { get; set; }
        public string PositionCode { get; set; } = null!;
        public int? KitNumber { get; set; }
        public int Age { get; set; }
        public string Country { get; set; } = "";
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
        public decimal Price { get; set; }
        public string? TeamName { get; set; }
        public string AvatarFileName { get; set; } = null!;
        public string AvatarUrl { get; set; } = string.Empty;
        public ICollection<PlayerAttributeDto> Attributes { get; set; } = new List<PlayerAttributeDto>();
        public ICollection<PlayerSeasonStatsDto> SeasonStats { get; set; } = new List<PlayerSeasonStatsDto>();
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
    }

    public class PlayerStatsDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public int? Goals { get; set; }
        public int? Matches { get; set; }
    }
}
