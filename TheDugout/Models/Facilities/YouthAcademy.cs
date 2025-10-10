namespace TheDugout.Models.Facilities
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Teams;
    public class YouthAcademy
    {
        public int Id { get; set; }

        public int? TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int Level { get; set; } = 1;
    }
}
