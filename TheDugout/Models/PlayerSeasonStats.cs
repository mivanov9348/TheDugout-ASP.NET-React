namespace TheDugout.Models
{
    public class PlayerSeasonStats
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; }

        public int MatchesPlayed { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
    }
}
