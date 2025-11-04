namespace TheDugout.Models.Teams
{
    using TheDugout.Models.Common;
    using TheDugout.Models.Leagues;
    public class TeamTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public int Popularity { get; set; } = 50;
        public int? CountryId { get; set; }
        public Country? Country { get; set; }
        public string? CountryCode { get; set; }
        public int? LeagueId { get; set; }              
        public LeagueTemplate? League { get; set; } = null!;
    }
}
