using TheDugout.Models.Teams;

namespace TheDugout.Models.Facilities
{
    public class YouthAcademy
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        public int Level { get; set; } = 1;
    }
}
