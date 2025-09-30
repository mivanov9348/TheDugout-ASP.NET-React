using TheDugout.Models.Teams;

namespace TheDugout.Models.Cups
{
    public class CupTeam
    {
        public int Id { get; set; }

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public bool IsEliminated { get; set; } = false;

    }
}
