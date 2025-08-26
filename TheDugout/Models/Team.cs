namespace TheDugout.Models
{
    public class Team
    {
        public int Id { get; set; }

        public int TemplateId { get; set; }
        public TeamTemplate Template { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;


        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;

        public int Points { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();


    }
}
