namespace TheDugout.Models
{
    public class LeagueStanding
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public int LeagueId { get; set; }
        public League League { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int Points { get; set; } = 0;
        public int Matches { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int GoalsFor { get; set; } = 0;
        public int GoalsAgainst { get; set; } = 0;
        public int GoalDifference { get; set; } = 0;

        public int Ranking { get; set; } = 0;
    }
}
