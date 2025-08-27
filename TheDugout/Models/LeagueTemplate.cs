namespace TheDugout.Models
{
    public class LeagueTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string LeagueCode { get; set; } = null!;

        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;

        public int Tier { get; set; }
        public int TeamsCount { get; set; }
        public int RelegationSpots { get; set; }
        public int PromotionSpots { get; set; }

        public ICollection<TeamTemplate> TeamTemplates { get; set; } = new List<TeamTemplate>();
    }

}
