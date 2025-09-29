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
            var events = _context.EventTypes
                .Include(e => e.Outcomes)
                .Include(e => e.AttributeWeights)
                    .ThenInclude(w => w.Attribute)
                .ToList();

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

            Console.WriteLine($"--- Calculating outcome for {player.FirstName} {player.LastName} (Age {player.Age}), Event {eventType.Code} ---");
            Console.WriteLine("Attributes used:");

            foreach (var weight in eventType.AttributeWeights)
            {
                var playerAttr = player.Attributes.FirstOrDefault(a => a.Attribute.Code == weight.AttributeCode);
                if (playerAttr != null)
                {
                    double contrib = playerAttr.Value * weight.Weight;
                    Console.WriteLine($" - {weight.AttributeCode}: Value={playerAttr.Value}, Weight={weight.Weight}, Contrib={contrib:F2}");
                    weightedSum += contrib;
                }
                else
                {
                    Console.WriteLine($" - {weight.AttributeCode}: NOT FOUND in player attributes!");
                }
            }

            // 1. Нормализиран атрибутен скор (1-20) → преобразуван в 0-100 чрез нелинейна (степенна) функция
            double attrScore = weightedSum / totalWeight; // средно между 1 и 20
            double baseScore = Math.Pow(attrScore / 20.0, 0.7) * 100.0; // нелинейна скала – по-високи стойности за средни/добри атрибути

            // 2. Възрастов фактор – пик около 27г, но по-мек penalty
            int ageRef = 27;
            int ageSpan = 10;
            double ageFactor = 1.0 - (Math.Abs(player.Age - ageRef) / (double)ageSpan) * 0.06; // намален ефект
            ageFactor = Math.Max(0.8, ageFactor); // минимум 80% (вместо 60%)

            double ageAdjusted = baseScore * ageFactor;

            // 3. Рандом вариация – по-малка и по-стабилна
            double variability = 4 + (Math.Abs(player.Age - ageRef) * 0.3); // намалена база и възрастов ефект
            double randomOffset = (_random.NextDouble() * 2 - 1) * variability;

            // 4. Финален скор
            double finalScore = ageAdjusted + randomOffset;
            int score = Math.Clamp((int)Math.Round(finalScore), 1, 100);

            Console.WriteLine($"WeightedSum={weightedSum:F2}, TotalWeight={totalWeight:F2}");
            Console.WriteLine($"AttrScore (1-20)={attrScore:F2}, BaseScore (0-100)={baseScore:F2}");
            Console.WriteLine($"AgeFactor={ageFactor:F2}, AfterAge={ageAdjusted:F2}");
            Console.WriteLine($"RandomOffset={randomOffset:F2}, FinalScore={finalScore:F2}, FinalScore Clamped={score}");

            var outcome = eventType.Outcomes
                .FirstOrDefault(o => score >= o.RangeMin && score <= o.RangeMax);

            if (outcome == null)
                throw new InvalidOperationException(
                    $"No outcome found for score {score} in event type {eventType.Code}");

            Console.WriteLine($"Outcome={outcome.Name}");
            Console.WriteLine("--------------------------------------------------------------");

            return outcome;
        }
        public string GetRandomCommentary(EventOutcome outcome, Models.Players.Player player)
        {
            var templates = _context.CommentaryTemplates
                .Where(c => c.EventOutcomeId == outcome.Id)
                .ToList();

            if (templates == null || templates.Count == 0)
                throw new InvalidOperationException($"No commentary templates found for outcome {outcome.Name}");

            int index = _random.Next(templates.Count);
            var template = templates[index];

            // Взимаме отбора на играча
            var team = player.Team
                ?? throw new InvalidOperationException($"Player {player.Id} has no team assigned.");

            string rendered = template.Template
                .Replace("{PlayerName}", $"{player.FirstName} {player.LastName}")
                .Replace("{TeamAbbr}", team.Abbreviation ?? team.Name); 

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
