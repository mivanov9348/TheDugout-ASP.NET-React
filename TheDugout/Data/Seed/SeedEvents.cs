namespace TheDugout.Data.Seed
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Models.Matches;

    public static class SeedEvents
    {
        public static async Task RunAsync(DugoutDbContext db, string seedDir, ILogger logger)
        {
            var eventTypesFile = Path.Combine(seedDir, "eventTypes.json");
            var eventTypes = await SeedData.ReadJsonAsync<List<SeedDtos.EventTypeDto>>(eventTypesFile);
            var dbEventTypes = await db.EventTypes.ToListAsync();

            foreach (var et in eventTypes)
            {
                var existing = dbEventTypes.FirstOrDefault(x => x.Code == et.Code);
                if (existing == null)
                {
                    db.EventTypes.Add(new EventType
                    {
                        Code = et.Code,
                        Name = et.Name
                    });
                }
                else
                {
                    if (existing.Name != et.Name)
                    {
                        existing.Name = et.Name;
                        db.EventTypes.Update(existing);
                    }
                }
            }
            await db.SaveChangesAsync();

            // EventAttributeWeights
            var eventWeightsFile = Path.Combine(seedDir, "matchEventWeights.json");
            var eventWeights = await SeedData.ReadJsonAsync<List<SeedDtos.EventAttributeWeightDto>>(eventWeightsFile);

            var dbEventTypesWithAttrs = await db.EventTypes
                .Include(et => et.AttributeWeights)
                .ToListAsync();

            var dbAttributes = await db.Attributes.ToListAsync();

            foreach (var ew in eventWeights)
            {
                var eventType = dbEventTypesWithAttrs.FirstOrDefault(x => x.Code == ew.EventTypeCode);
                if (eventType == null) continue;

                foreach (var attr in ew.Attributes)
                {
                    var existing = eventType.AttributeWeights.FirstOrDefault(x => x.AttributeCode == attr.AttributeCode);
                    if (existing == null)
                    {
                        var attribute = dbAttributes.FirstOrDefault(a => a.Code == attr.AttributeCode);
                        if (attribute == null) continue;

                        db.EventAttributeWeights.Add(new EventAttributeWeight
                        {
                            EventTypeCode = ew.EventTypeCode,
                            EventType = eventType,
                            AttributeCode = attr.AttributeCode,
                            Attribute = attribute,
                            Weight = attr.Weight
                        });
                    }
                    else
                    {
                        if (Math.Abs(existing.Weight - attr.Weight) > 0.0001)
                        {
                            existing.Weight = attr.Weight;
                            db.EventAttributeWeights.Update(existing);
                        }
                    }
                }
            }

            await db.SaveChangesAsync();

            // EventOutcomes
            var outcomesFile = Path.Combine(seedDir, "eventOutcomes.json");
            var eventOutcomes = await SeedData.ReadJsonAsync<List<SeedDtos.EventOutcomeDto>>(outcomesFile);
            var dbOutcomes = await db.EventOutcomes.Include(o => o.EventType).ToListAsync();
            var typesByCode = await db.EventTypes.ToDictionaryAsync(x => x.Code);

            foreach (var eo in eventOutcomes)
            {
                if (!typesByCode.TryGetValue(eo.EventTypeCode, out var type))
                {
                    logger.LogWarning("Unknown EventTypeCode '{EventTypeCode}' for outcome '{Name}'", eo.EventTypeCode, eo.Name);
                    continue;
                }

                // Търсим по EventTypeId и Range (уникалната комбинация за outcome), вместо само по име
                var existing = dbOutcomes.FirstOrDefault(x => x.EventTypeId == type.Id && x.Name == eo.Name);

                if (existing == null)
                {
                    // Ако няма → създаваме
                    db.EventOutcomes.Add(new EventOutcome
                    {
                        Name = eo.Name,
                        EventTypeId = type.Id,
                        EventTypeCode = eo.EventTypeCode,
                        ChangesPossession = eo.ChangesPossession,
                        RangeMin = eo.RangeMin,
                        RangeMax = eo.RangeMax
                    });
                }
                else
                {
                    // Ако има → презаписваме всички полета (не само част от тях)
                    existing.Name = eo.Name;
                    existing.EventTypeId = type.Id;
                    existing.EventTypeCode = eo.EventTypeCode;
                    existing.ChangesPossession = eo.ChangesPossession;
                    existing.RangeMin = eo.RangeMin;
                    existing.RangeMax = eo.RangeMax;

                    db.EventOutcomes.Update(existing);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
