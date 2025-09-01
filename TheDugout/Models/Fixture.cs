namespace TheDugout.Models
{
    public class Fixture
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int LeagueId { get; set; }
        public League League { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        public int HomeTeamGoals { get; set; }
        public int AwayTeamGoals { get; set; }

        public DateTime Date { get; set; }

        public int Round { get; set; }



    }
}
