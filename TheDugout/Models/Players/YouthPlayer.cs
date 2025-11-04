namespace TheDugout.Models.Players
{
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;

    public class YouthPlayer
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int YouthAcademyId { get; set; }
        public YouthAcademy YouthAcademy { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public bool IsPromoted { get; set; } = false;

     
    }

}
