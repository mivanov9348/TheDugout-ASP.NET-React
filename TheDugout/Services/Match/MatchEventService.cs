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
        public EventOutcome GetEventOutcome(Models.Players.Player player, EventType eventType)
        {
            if (eventType.AttributeWeights == null || !eventType.AttributeWeights.Any())
                throw new InvalidOperationException($"No attribute weights found for event type {eventType.Code}");

            double weightedSum = 0;
            double totalWeight = eventType.AttributeWeights.Sum(w => w.Weight);

            foreach (var weight in eventType.AttributeWeights)
            {
                var playerAttr = player.Attributes.FirstOrDefault(a => a.Attribute.Code == weight.AttributeCode);
                if (playerAttr != null)
                {
                    weightedSum += (playerAttr.Value / 100.0) * weight.Weight;
                }
            }

            double normalized = weightedSum / totalWeight;

            double baseScore = normalized * 100.0;

            double Rbase = 6.0;
            int ageRef = 27;
            int ageSpan = 12;
            double Rmin = 2.0;
            double Rmax = 12.0;

            double ageFactor = 1.0 + (ageRef - player.Age) / (double)ageSpan;
            double R = Rbase * ageFactor;
            R = Math.Max(Rmin, Math.Min(Rmax, R));

            double offset = (_random.NextDouble() * 2 - 1) * R; 
            double score = baseScore + offset;

            int finalScore = Math.Max(1, Math.Min(100, (int)Math.Round(score)));

            var outcome = eventType.Outcomes
                .FirstOrDefault(o => finalScore >= o.RangeMin && finalScore <= o.RangeMax);

            if (outcome == null)
                throw new InvalidOperationException(
                    $"No outcome found for score {finalScore} in event type {eventType.Code}");

            return outcome;
        }

        public string GetRandomCommentary(EventOutcome outcome, Models.Players.Player player)
        {
            if (outcome.CommentaryTemplates == null || !outcome.CommentaryTemplates.Any())
                throw new InvalidOperationException($"No commentary templates found for outcome {outcome.Name}");

            int index = _random.Next(outcome.CommentaryTemplates.Count);
            var template = outcome.CommentaryTemplates.ElementAt(index);

            string rendered = template.Template.Replace("{PlayerName}", $"{player.FirstName} {player.LastName}");

            return rendered;
        }
        public MatchEvent CreateMatchEvent(int matchId, int minute, Models.Teams.Team team, Models.Players.Player player, EventType eventType, EventOutcome outcome, string commentary)
        {
            var matchEvent = new MatchEvent
            {
                MatchId = matchId,
                Minute = minute,
                TeamId = team.Id,
                Team = team,
                PlayerId = player.Id,
                Player = player,
                EventTypeId = eventType.Id,
                EventType = eventType,
                OutcomeId = outcome.Id,
                Outcome = outcome,
                Commentary = commentary
            };

            _context.MatchEvents.Add(matchEvent);
            _context.SaveChanges();

            return matchEvent;
        }


    }
}
