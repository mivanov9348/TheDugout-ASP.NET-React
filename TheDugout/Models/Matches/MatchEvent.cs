using TheDugout.Models.Players;
using TheDugout.Models.Teams;

namespace TheDugout.Models.Matches
{
    public class MatchEvent
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public int Minute { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int EventTypeId { get; set; }
        public EventType EventType { get; set; } = null!;

        public int OutcomeId { get; set; }
        public EventOutcome Outcome { get; set; } = null!;

        public string Commentary { get; set; } = string.Empty;
    }
}
