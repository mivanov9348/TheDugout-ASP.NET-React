using TheDugout.DTOs.Player;

namespace TheDugout.DTOs.Team
{
    public class TeamDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public ICollection<PlayerDto> Players { get; set; } = new List<PlayerDto>();
    }

    public class SetTacticRequest
    {
        public int TacticId { get; set; }
        public string? CustomName { get; set; }
        public Dictionary<string, string?> Lineup { get; set; } = new();
        public Dictionary<string, string?>? Substitutes { get; set; }

    }
}
