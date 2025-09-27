using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Match
{
    public interface IMatchEventService
    {
        EventType GetRandomEvent();
        EventOutcome GetEventOutcome(Player player, EventType eventType);
        CommentaryTemplate GetRandomCommentary(EventOutcome outcome);
    }

}
