namespace TheDugout.Models.Players
{
    using TheDugout.Models.Facilities;
    public class YouthPlayer
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int YouthAcademyId { get; set; }
        public YouthAcademy YouthAcademy { get; set; } = null!;

        public bool IsPromoted { get; set; } = false;

     
    }

}
