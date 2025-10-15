namespace TheDugout.Services.Match.Interfaces
{
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    public interface IMatchEventService
    {
        EventType GetRandomEvent();
        EventType GetEventByCode(string code);
        EventOutcome GetEventOutcome(Player player, EventType eventType);
        EventOutcome GetPenaltyOutcome(Player kicker, Player goalkeeper, EventType eventType);
        string GetRandomCommentary(EventOutcome outcome, Player player);
        Task<MatchEvent> CreateMatchEvent(int matchId, int minute, Models.Teams.Team team, Player player, EventType eventType, EventOutcome outcome, string commentary);
    }
}
