namespace TheDugout.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
        public string Position { get; set; } = null!;
        public int JerseyNumber { get; set; }
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
        public bool IsActive { get; set; }
    }
}
