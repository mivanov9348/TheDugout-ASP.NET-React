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
            if (eventType.AttributeWeights == null || !eventType.AttributeWeights.Any())
                throw new InvalidOperationException($"No attribute weights found for event type {eventType.Code}");

            // 1. Calculate weighted attribute sum (normalized between 0 and 1)
            double weightedSum = 0;
            double totalWeight = eventType.AttributeWeights.Sum(w => w.Weight);

            foreach (var weight in eventType.AttributeWeights)
            {
                var playerAttr = player.Attributes.FirstOrDefault(a => a.Attribute.Code == weight.AttributeCode);
                if (playerAttr != null)
                {
                    // Attribute.Value is expected to be 1–100
                    weightedSum += (playerAttr.Value / 100.0) * weight.Weight;
                }
            }

            double normalized = weightedSum / totalWeight;

            // 2. Base score (0–100 scale)
            double baseScore = normalized * 100.0;

            // 3. Calculate random offset range depending on age
            double Rbase = 6.0;
            int ageRef = 27;
            int ageSpan = 12;
            double Rmin = 2.0;
            double Rmax = 12.0;

            double ageFactor = 1.0 + (ageRef - player.Age) / (double)ageSpan;
            double R = Rbase * ageFactor;
            R = Math.Max(Rmin, Math.Min(Rmax, R));

            // 4. Apply random offset
            double offset = (_random.NextDouble() * 2 - 1) * R; // range [-R, +R]
            double score = baseScore + offset;

            // 5. Clamp between 1 and 100
            int finalScore = Math.Max(1, Math.Min(100, (int)Math.Round(score)));

            // 6. Pick the outcome that matches finalScore
            var outcome = eventType.Outcomes
                .FirstOrDefault(o => finalScore >= o.RangeMin && finalScore <= o.RangeMax);

            if (outcome == null)
                throw new InvalidOperationException(
                    $"No outcome found for score {finalScore} in event type {eventType.Code}");

            return outcome;
        }

        public CommentaryTemplate GetRandomCommentary(EventOutcome outcome)
        {
            if (outcome.CommentaryTemplates == null || !outcome.CommentaryTemplates.Any())
                throw new InvalidOperationException($"No commentary templates found for outcome {outcome.Name}");

            int index = _random.Next(outcome.CommentaryTemplates.Count);
            return outcome.CommentaryTemplates.ElementAt(index);
        }


    }
}
