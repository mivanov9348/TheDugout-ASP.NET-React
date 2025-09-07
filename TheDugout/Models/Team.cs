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

        public string LogoFileName { get; set; } = "default_logo.png";

        public int? LeagueId { get; set; }
        public League? League { get; set; }

        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;

        public int Points { get; set; } = 0;
        public int Matches { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int GoalsFor { get; set; } = 0;
        public int GoalsAgainst { get; set; } = 0;
        public int GoalDifference { get; set; } = 0;
        public decimal Balance { get; set; }
        public int Popularity { get; set; } = 10;

        public ICollection<FinancialTransaction> TransactionsFrom { get; set; } = new List<FinancialTransaction>();
        public ICollection<FinancialTransaction> TransactionsTo { get; set; } = new List<FinancialTransaction>();

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<Fixture> HomeFixtures { get; set; } = new List<Fixture>();
        public ICollection<Fixture> AwayFixtures { get; set; } = new List<Fixture>();

        // 🆕 връзката 1:1 с тактика
        public TeamTactic? TeamTactic { get; set; }
    }

}
