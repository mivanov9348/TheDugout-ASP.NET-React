namespace TheDugout.Models.Common
{
    public class Region
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   
        public string Name { get; set; } = null!; 

        public ICollection<Country> Countries { get; set; } = new List<Country>();
        public ICollection<FirstName> FirstNames { get; set; } = new List<FirstName>();
        public ICollection<LastName> LastNames { get; set; } = new List<LastName>();
    }
}
