using TheDugout.DTOs.Player;

namespace TheDugout.DTOs.Team
{
    public class TeamDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public ICollection<PlayerDto> Players { get; set; } = new List<PlayerDto>();
    }

}
