namespace TheDugout.Models
{
    public class TeamTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;        
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
        public string? CountryCode { get; set; }
        public int? LeagueId { get; set; }              
        public LeagueTemplate? League { get; set; } = null!;
    }
}
