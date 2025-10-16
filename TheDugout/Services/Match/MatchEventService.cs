namespace TheDugout.Services.Match
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    using TheDugout.Services.Match.Interfaces;

    public class MatchEventService : IMatchEventService
    {
        private static readonly Random _random = new Random();
        private readonly DugoutDbContext _context;
        private readonly Dictionary<string, EventType> _eventCache = new();

        public MatchEventService(DugoutDbContext context)
        {
            _context = context;
        }
        public EventType GetRandomEvent()
        {
            // Get all EventType IDs
            var eventIds = _context.EventTypes.Select(e => e.Id).ToList();

            if (eventIds.Count == 0)
                throw new InvalidOperationException("No event types found in database.");

            int randomId = eventIds[_random.Next(eventIds.Count)];

            // Load the EventType with related data
            var ev = _context.EventTypes
                .AsNoTracking()
                .Include(e => e.Outcomes)
                .Include(e => e.AttributeWeights)
                    .ThenInclude(w => w.Attribute)
                .First(e => e.Id == randomId);

            return ev;
        }
        public EventOutcome GetEventOutcome(Player player, EventType eventType)
        {
            var attributeWeights = eventType.AttributeWeights?.Any() == true
                ? eventType.AttributeWeights
                : _context.EventAttributeWeights
                    .AsNoTracking()
                    .Where(w => w.EventType.Id == eventType.Id)
                    .ToList();

            if (attributeWeights.Count == 0)
            {
                return eventType.Outcomes.FirstOrDefault()
                       ?? new EventOutcome { Name = "Default", RangeMin = 0, RangeMax = 100 };
            }

            // Превръщаме player.Attributes в речник
            var playerAttrDict = player.Attributes
                .ToDictionary(a => a.Attribute.Code, a => a.Value);

            double weightedSum = 0;
            double totalWeight = 0;

            foreach (var weight in attributeWeights)
            {
                if (playerAttrDict.TryGetValue(weight.AttributeCode, out var attrValue))
                {
                    weightedSum += attrValue * weight.Weight;
                    totalWeight += weight.Weight;
                }
            }   

            if (totalWeight == 0)
                totalWeight = 1; // предпазна мярка

            double attrScore = weightedSum / totalWeight;
            double baseScore = Math.Pow(attrScore / 20.0, 0.85) * 100.0;

            if (eventType.Code == "SHT")
            {
                baseScore *= 1.15;
            }

            // age factor
            const int ageRef = 27;
            const int ageSpan = 10;
            double ageFactor = 1.0 - (Math.Abs(player.Age - ageRef) / (double)ageSpan) * 0.06;
            ageFactor = Math.Clamp(ageFactor, 0.8, 1.0);

            // random variability
            double variability = 6 + Math.Abs(player.Age - 27) * 0.3;
            double randomOffset = (_random.NextDouble() * 2 - 1) * variability;

            int score = Math.Clamp((int)Math.Round(baseScore * ageFactor + randomOffset), 1, 100);

            var outcome = eventType.Outcomes
                .FirstOrDefault(o => score >= o.RangeMin && score <= o.RangeMax)
                ?? eventType.Outcomes.FirstOrDefault()
                ?? new EventOutcome { Name = "Default", RangeMin = 0, RangeMax = 100 };

            return outcome;
        }

        public EventOutcome GetPenaltyOutcome(Player kicker, Player goalkeeper, EventType eventType)
        {
            // cache attributes in dictionaries for quick access
            var kickerAttributes = kicker.Attributes.ToDictionary(a => a.Attribute.Code, a => a.Value);
            var keeperAttributes = goalkeeper.Attributes;

            // Calculation steps:
            double weightedSum = 0, totalWeight = 0;

            foreach (var w in eventType.AttributeWeights)
            {
                if (kickerAttributes.TryGetValue(w.AttributeCode, out var value))
                {
                    weightedSum += value * w.Weight;
                    totalWeight += w.Weight;
                }
            }

            double kickerScore = totalWeight > 0 ? weightedSum / totalWeight : 10;

            // Calculating goalkeeper score
            var gkValues = keeperAttributes
                .Where(a => a.Attribute.Category == AttributeCategory.Goalkeeping)
                .Select(a => a.Value)
                .ToList();

            double keeperScore = gkValues.Count > 0 ? gkValues.Average() : 10;

            // Calculating base chance
            double baseChance = 50 + (kickerScore - keeperScore) * 1.2;

            // Psychological factors
            double composure = kickerAttributes.TryGetValue("COM", out var comVal) ? comVal : 10;
            double pressureFactor = 1 + ((composure - 10) / 100.0);
            double randomOffset = (_random.NextDouble() * 10) - 5;

            double finalChance = (baseChance * pressureFactor) + randomOffset;
            finalChance = Math.Clamp(finalChance, 1, 99);

            // Save chance for goalkeeper
            double heroSaveChance = 5 + ((keeperScore - 10) * 0.5);
            if (_random.NextDouble() * 100 < heroSaveChance)
            {
                finalChance -= _random.NextDouble() * 15 + 5;
                finalChance = Math.Max(1, finalChance);
#if DEBUG
                Console.WriteLine($"🦸‍♂️ GK Hero Save triggered! ({heroSaveChance:F1}% chance)");
#endif
            }

            // Retrieve outcome based on final chance
            var outcome = eventType.Outcomes
                .FirstOrDefault(o => finalChance >= o.RangeMin && finalChance <= o.RangeMax)
                ?? eventType.Outcomes.First();

            return outcome;
        }
        public string GetRandomCommentary(EventOutcome outcome, Player player)
        {
            var templates = _context.CommentaryTemplates
                .AsNoTracking()
                .Where(c => c.EventOutcomeId == outcome.Id)
                .Select(c => c.Template)
                .ToList();

            if (templates.Count == 0)
                return $"No commentary available for {outcome.Name}."; // безопасен fallback

            string template = templates[_random.Next(templates.Count)];

            var team = player.Team ?? throw new InvalidOperationException($"Player {player.Id} has no team assigned.");

            return template
                .Replace("{PlayerName}", $"{player.FirstName} {player.LastName}")
                .Replace("{TeamAbbr}", team.Abbreviation ?? team.Name);
        }

        public async Task<MatchEvent> CreateMatchEvent(int matchId, int minute, Models.Teams.Team team, Models.Players.Player player, EventType eventType, EventOutcome outcome, string commentary)
        {
            var matchEvent = new MatchEvent
            {
                MatchId = matchId,
                Minute = minute,
                TeamId = team.Id,
                GameSaveId = team.GameSaveId,
                PlayerId = player.Id,
                EventTypeId = eventType.Id,
                OutcomeId = outcome.Id,
                Commentary = commentary
            };

            _context.MatchEvents.Add(matchEvent);
            await _context.SaveChangesAsync();

            return matchEvent;
        }
        public EventType GetEventByCode(string code)
        {
            if (_eventCache.TryGetValue(code, out var cached))
                return cached;

            var entity = _context.EventTypes
                .AsNoTracking()
                .Include(e => e.Outcomes)
                .Include(e => e.AttributeWeights)
                    .ThenInclude(w => w.Attribute)
                .FirstOrDefault(e => e.Code == code)
                ?? throw new InvalidOperationException($"EventType with code '{code}' not found.");

            _eventCache[code] = entity;
            return entity;
        }
    }
}
