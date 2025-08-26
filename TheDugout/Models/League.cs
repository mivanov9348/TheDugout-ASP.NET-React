namespace TheDugout.Models
{
    public class League
    {
        public int Id { get; set; }

        public int TemplateId { get; set; }
        public LeagueTemplate Template { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;

        public int Tier { get; set; }
        public int TeamsCount { get; set; }
        public int RelegationSpots { get; set; }
        public int PromotionSpots { get; set; }

        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}
