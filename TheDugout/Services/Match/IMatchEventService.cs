using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Match
{
    public interface IMatchEventService
    {
        EventType GetRandomEvent();
        EventType GetEventByCode(string code);
        EventOutcome GetEventOutcome(Models.Players.Player player, EventType eventType);
        string GetRandomCommentary(EventOutcome outcome, Models.Players.Player player);
        MatchEvent CreateMatchEvent(int matchId, int minute, Models.Teams.Team team, Models.Players.Player player, EventType eventType, EventOutcome outcome, string commentary);
    };

}
