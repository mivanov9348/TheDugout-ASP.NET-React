namespace TheDugout.Data.DtoNewGame
{
    public class TeamTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public int CountryId { get; set; }

        public int LeagueId { get; set; }       
        public string LeagueName { get; set; } 
        public int Tier { get; set; }
    }
}
