using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services.Training;

public class TrainingService : ITrainingService
{
    private readonly DugoutDbContext _context;
    private readonly Random _random = new();
    private readonly ILogger<TrainingService> _logger;

    public TrainingService(DugoutDbContext context, ILogger<TrainingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PlayerTrainingAssignmentDto>> AutoAssignAttributesAsync(int teamId, int gameSaveId)
    {
        var players = await _context.Players
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
                    .ThenInclude(a => a.PositionWeights)
            .Where(p => p.TeamId == teamId && p.GameSaveId == gameSaveId)
            .ToListAsync();

        var assignments = new List<PlayerTrainingAssignmentDto>();

        foreach (var player in players)
        {
            var bestAttr = player.Attributes
                .OrderByDescending(pa =>
                {
                    var weight = pa.Attribute.PositionWeights
                        .FirstOrDefault(w => w.PositionId == player.PositionId)?.Weight ?? 0;

                    // Може да се добави бонус за ниска текуща стойност:
                    var bonusForLowValue = 1.0 / (pa.Value + 1);
                    return weight + bonusForLowValue;
                })
                .FirstOrDefault();

            if (bestAttr != null)
            {
                assignments.Add(new PlayerTrainingAssignmentDto
                {
                    PlayerId = player.Id,
                    AttributeId = bestAttr.AttributeId
                });
            }
        }

        return assignments;
    }

    public async Task<List<TrainingResultDto>> RunTrainingSessionAsync(
        int gameSaveId,
        int teamId,
        int seasonId,
        DateTime date,
        List<PlayerTrainingAssignmentDto> assignments)
    {
        if (assignments == null || !assignments.Any())
            throw new InvalidOperationException("No assignments provided.");

        // проверка за вече провеждана тренировка (по gameSave + team + date)
        bool alreadyTrained = await _context.TrainingSessions
            .AnyAsync(ts => ts.GameSaveId == gameSaveId
                            && ts.TeamId == teamId
                            && ts.Date.Date == date.Date);
        if (alreadyTrained)
            throw new InvalidOperationException("Вече е проведена тренировка за този ден.");

        // distinct playerIds
        var playerIds = assignments.Select(a => a.PlayerId).Distinct().ToList();

        // Опитваме строгото търсене (играчите трябва да са от този отбор и save)
        var players = await _context.Players
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
            .Where(p => playerIds.Contains(p.Id))
            .ToListAsync();

        if (players.Count != playerIds.Count)
        {
            // fallback: зареди по ID без филтър team/save
            _logger?.LogDebug("Strict load found {Found}/{Expected}. Falling back to load by ID.", players.Count, playerIds.Count);

            var playersById = await _context.Players
                .Include(p => p.Attributes)
                    .ThenInclude(pa => pa.Attribute)
                .Where(p => playerIds.Contains(p.Id))
                .ToListAsync();

            var missing = playerIds.Except(playersById.Select(p => p.Id)).ToList();
            if (missing.Any())
            {
                // ако има липсващи id-та в базата — грешка и списък
                throw new InvalidOperationException($"Играч(и) с id [{string.Join(", ", missing)}] не бяха намерени в базата.");
            }

            // ако всички id-та съществуват, но някои са в друг отбор/сейв — логваме warning, но продължаваме
            var mismatched = playersById
                .Where(p => p.TeamId != teamId || p.GameSaveId != gameSaveId)
                .Select(p => new { p.Id, p.TeamId, p.GameSaveId })
                .ToList();

            if (mismatched.Any())
            {
                _logger?.LogWarning("Намерени играчи, но с различен TeamId/GameSaveId: {Mismatches}",
                    string.Join(", ", mismatched.Select(m => $"(id:{m.Id},team:{m.TeamId},save:{m.GameSaveId})")));
            }

            // използваме playersById (вече всички съществуват)
            players = playersById;
        }

        // Създаваме сесия
        var trainingSession = new TrainingSession
        {
            GameSaveId = gameSaveId,
            TeamId = teamId,
            SeasonId = seasonId,
            Date = date,
            PlayerTrainings = new List<PlayerTraining>()
        };

        var results = new List<TrainingResultDto>();

        // Обработваме всяка задача (assignment)
        foreach (var assignment in assignments)
        {
            var player = players.FirstOrDefault(p => p.Id == assignment.PlayerId);
            if (player == null)
            {
                // Това не би трябвало да се случи след горната проверка, но guard-ване
                throw new InvalidOperationException($"Играч с id {assignment.PlayerId} липсва след зареждане.");
            }

            var pa = player.Attributes.FirstOrDefault(a => a.AttributeId == assignment.AttributeId);
            if (pa == null)
            {
                // няма такъв атрибут у играча
                throw new InvalidOperationException($"Играч {player.Id} няма атрибут {assignment.AttributeId}.");
            }

            var oldValue = pa.Value;

            // --- ФОРМУЛА (бавно, положително развитие)
            double baseGain = 0.03; // базов прогрес на сесия
            double ageFactor = player.Age < 21 ? 1.3 : player.Age > 28 ? 0.7 : 1.0;
            double randomFactor = _random.NextDouble() * 0.3 + 0.85; // 0.85 - 1.15
            double gain = baseGain * ageFactor * randomFactor;

            // Нотираме прогреса (progress трябва да е поле double в PlayerAttribute)
            pa.Progress += gain;

            int changeValue = 0;
            if (pa.Progress >= 1.0)
            {
                pa.Progress -= 1.0; // запазваме остатъка, вместо да чистим на 0
                pa.Value += 1;
                changeValue = 1;
            }

            trainingSession.PlayerTrainings.Add(new PlayerTraining
            {
                PlayerId = player.Id,
                AttributeId = pa.AttributeId,
                ChangeValue = changeValue
            });

            results.Add(new TrainingResultDto
            {
                PlayerId = player.Id,
                PlayerName = $"{player.FirstName} {player.LastName}",
                AttributeName = pa.Attribute.Name,
                OldValue = oldValue,
                NewValue = pa.Value,
                ProgressGain = Math.Round(gain, 3)
            });
        }

        _context.TrainingSessions.Add(trainingSession);
        await _context.SaveChangesAsync();

        return results;
    }
}
