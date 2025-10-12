namespace TheDugout.Services.Match
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Players;
    public class MatchEventService : IMatchEventService
    {
        private static readonly Random _random = new Random();
        private readonly DugoutDbContext _context;
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
                // Вземаме атрибутните тежести директно от контекста, ако липсват в самия eventType
                var attributeWeights = eventType.AttributeWeights?.Any() == true
                    ? eventType.AttributeWeights
                    : _context.EventAttributeWeights.Where(w => w.EventType.Id == eventType.Id).ToList();

                if (attributeWeights == null || !attributeWeights.Any())
                {
                    Console.WriteLine($"⚠ Няма намерени атрибутни тежести за евент {eventType.Code}. Връщам дефолтен изход.");
                    // Можеш да върнеш дефолтен outcome (пример: първия), или null
                    return eventType.Outcomes.FirstOrDefault()
                           ?? new EventOutcome { Name = "Default", RangeMin = 0, RangeMax = 100 };
                }

                double weightedSum = 0;
                double totalWeight = attributeWeights.Sum(w => w.Weight);
               
                foreach (var weight in attributeWeights)
                {
                    var playerAttr = player.Attributes.FirstOrDefault(a => a.Attribute.Code == weight.AttributeCode);
                    if (playerAttr != null)
                    {
                        double contrib = playerAttr.Value * weight.Weight;
                        weightedSum += contrib;
                    }
                    else
                    {
                        Console.WriteLine($" - {weight.AttributeCode}: NOT FOUND in player attributes!");
                    }
                }

                // 1. Нормализиран скор (1–20) → скалиран в 0–100
                double attrScore = weightedSum / totalWeight;
                double baseScore = Math.Pow(attrScore / 20.0, 0.7) * 100.0;

                // 2. Възрастов фактор
                int ageRef = 27;
                int ageSpan = 10;
                double ageFactor = 1.0 - (Math.Abs(player.Age - ageRef) / (double)ageSpan) * 0.06;
                ageFactor = Math.Max(0.8, ageFactor);

                double ageAdjusted = baseScore * ageFactor;

                // 3. Рандом вариация
                double variability = 4 + (Math.Abs(player.Age - ageRef) * 0.3);
                double randomOffset = (_random.NextDouble() * 2 - 1) * variability;

                // 4. Финален скор
                double finalScore = ageAdjusted + randomOffset;
                int score = Math.Clamp((int)Math.Round(finalScore), 1, 100);
                       

                var outcome = eventType.Outcomes
                    .FirstOrDefault(o => score >= o.RangeMin && score <= o.RangeMax);

                if (outcome == null)
                {
                    Console.WriteLine($"⚠ Няма outcome за score {score} → ще върна дефолтен.");
                    outcome = eventType.Outcomes.FirstOrDefault()
                              ?? new EventOutcome { Name = "Default", RangeMin = 0, RangeMax = 100 };
                }

                Console.WriteLine($"Outcome={outcome.Name}");
                Console.WriteLine("--------------------------------------------------------------");

                return outcome;
            }

        public EventOutcome GetPenaltyOutcome(Models.Players.Player kicker, Models.Players.Player goalkeeper, EventType eventType)
        {
            // 🧩 Помощна функция за взимане на стойност на атрибут
            double GetAttrValue(Models.Players.Player player, string code, double def = 10)
                => player?.Attributes.FirstOrDefault(a => a.Attribute.Code == code)?.Value ?? def;

            // 🧠 1. Зареждаме теглата за дадения EventType (примерно "PEN")
            var weights = eventType.AttributeWeights.ToList();

            // ⚖️ 2. Делим теглата на такива, които важат за нападателя (всички)
            //    Вратарят ще се изчисли отделно по собствените му goalkeeping атрибути
            var kickerWeights = weights.ToList();

            // 🏃‍♂️ 3. Изчисляваме оценка за нападателя
            double kickerScore = 0, totalKickerWeight = 0;
            foreach (var w in kickerWeights)
            {
                double value = GetAttrValue(kicker, w.AttributeCode);
                kickerScore += value * w.Weight;
                totalKickerWeight += w.Weight;
            }
            kickerScore = totalKickerWeight > 0 ? kickerScore / totalKickerWeight : 10;

            // 🧱 4. Изчисляваме оценка за вратаря спрямо всички негови Goalkeeping атрибути
            var gkAttributes = goalkeeper.Attributes
                .Where(a => a.Attribute.Category == AttributeCategory.Goalkeeping)
                .ToList();

            double keeperScore = gkAttributes.Any()
                ? gkAttributes.Average(a => a.Value)
                : 10; // fallback ако няма такива атрибути

            // ⚖️ 5. Изчисляваме базов шанс за гол
            double baseChance = 50 + (kickerScore - keeperScore) * 1.2;

            // 😤 6. Психологически фактори и малко RNG
            double composure = GetAttrValue(kicker, "COM");
            double pressureFactor = 1 + ((composure - 10) / 100.0);
            double randomOffset = _random.NextDouble() * 10 - 5;

            double finalChance = (baseChance * pressureFactor) + randomOffset;
            finalChance = Math.Clamp(finalChance, 1, 99);

            // 🧤 7. Hero Save шанс – вратарят има 5% шанс да направи чудо
            double heroSaveChance = 5 + ((keeperScore - 10) * 0.5);
            if (_random.NextDouble() * 100 < heroSaveChance)
            {
                finalChance -= _random.NextDouble() * 15 + 5;
                finalChance = Math.Max(1, finalChance);
                Console.WriteLine($"🦸‍♂️ GK Hero Save triggered! ({heroSaveChance:F1}% chance)");
            }

            // 🎯 8. Определяме outcome според диапазоните
            var outcome = eventType.Outcomes
                .FirstOrDefault(o => finalChance >= o.RangeMin && finalChance <= o.RangeMax)
                ?? eventType.Outcomes.First();

            // 🪄 9. Debug лог
            Console.WriteLine($"⚽ Penalty: {kicker.FirstName} {kicker.LastName} vs GK {goalkeeper?.FirstName ?? "?"}");
            Console.WriteLine($"   KickerScore={kickerScore:F1}, KeeperScore={keeperScore:F1}, FinalChance={finalChance:F1}% → {outcome.Name}");

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
                var entity = _context.EventTypes
                    .Include(e => e.Outcomes)
                    .Include(e => e.AttributeWeights)
                        .ThenInclude(w => w.Attribute)
                    .AsNoTracking() 
                    .FirstOrDefault(e => e.Code == code)
                    ?? throw new InvalidOperationException($"EventType with code '{code}' not found.");

                return entity;
            }

    }
}
