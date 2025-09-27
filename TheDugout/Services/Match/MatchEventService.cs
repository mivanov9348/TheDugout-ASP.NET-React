using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Matches;
using TheDugout.Models.Players;

namespace TheDugout.Services.Match
{
    public class MatchEventService : IMatchEventService
    {
        private static readonly Random _random = new Random();
        private DugoutDbContext _context;
        public MatchEventService(DugoutDbContext context)
        {
            _context = context;
        }

        public EventType GetRandomEvent()
        {
            var events = _context.EventTypes.ToList();

            if (events.Count == 0)
                throw new InvalidOperationException("No event types found in database.");

            int index = _random.Next(events.Count);
            return events[index];
        }
        public EventOutcome GetEventOutcome(Player player, EventType eventType)
        {
            throw new NotImplementedException();
        }

        public CommentaryTemplate GetRandomCommentary(EventOutcome outcome)
        {
            throw new NotImplementedException();
        }
        
    }
}
