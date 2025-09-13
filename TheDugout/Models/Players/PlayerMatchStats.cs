namespace TheDugout.Models.Players
{
    public class PlayerMatchStats
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        // public int MatchId { get; set; }
        // public Match Match { get; set; }

        public int Goals { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int MinutesPlayed { get; set; }
    }
}
