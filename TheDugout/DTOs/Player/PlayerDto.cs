namespace TheDugout.DTOs.Player
{
    // Dtos/Player/PlayerDto.cs
    public class PlayerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Position { get; set; } = null!;
        public int KitNumber { get; set; }
        public int Age { get; set; }
        public string Country { get; set; } = "";
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
        public decimal Price { get; set; }

        public ICollection<PlayerAttributeDto> Attributes { get; set; } = new List<PlayerAttributeDto>();
        public ICollection<PlayerSeasonStatsDto> SeasonStats { get; set; } = new List<PlayerSeasonStatsDto>();
    }

    // Dtos/Player/PlayerAttributeDto.cs
    public class PlayerAttributeDto
    {
        public int AttributeId { get; set; }
        public string Name { get; set; } = null!;
        public int Value { get; set; }
    }

    // Dtos/Player/PlayerSeasonStatsDto.cs
    public class PlayerSeasonStatsDto
    {
        public int SeasonId { get; set; }
        public int MatchesPlayed { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
    }

}
