using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Models.Training;
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

    public async Task<List<AutoAssignResultDto>> AutoAssignAttributesAsync(int teamId, int gameSaveId)
    {
        var players = await _context.Players
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
                    .ThenInclude(a => a.PositionWeights)
            .Where(p => p.TeamId == teamId && p.GameSaveId == gameSaveId)
            .ToListAsync();

        var assignments = new List<AutoAssignResultDto>();

        foreach (var player in players)
        {
            var bestAttr = player.Attributes
                .OrderByDescending(pa =>
                {
                    var weight = pa.Attribute.PositionWeights
                        .FirstOrDefault(w => w.PositionId == player.PositionId)?.Weight ?? 0;

                    var bonusForLowValue = 1.0 / (pa.Value + 1);
                    return weight + bonusForLowValue;
                })
                .FirstOrDefault();

            if (bestAttr != null)
            {
                assignments.Add(new AutoAssignResultDto
                {
                    PlayerId = player.Id,
                    AttributeId = bestAttr.AttributeId,
                    AttributeName = bestAttr.Attribute.Name,
                    CurrentValue = bestAttr.Value
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

        date = date.ToUniversalTime().Date;

        bool alreadyTrained = await _context.TrainingSessions
            .AnyAsync(ts => ts.GameSaveId == gameSaveId
                            && ts.TeamId == teamId
                            && ts.Date == date);
        if (alreadyTrained)
            throw new InvalidOperationException("Вече е проведена тренировка за този ден.");

        var playerIds = assignments.Select(a => a.PlayerId).Distinct().ToList();
        var players = await _context.Players
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
            .Where(p => playerIds.Contains(p.Id))
            .ToListAsync();

        if (players.Count != playerIds.Count)
        {
            var missing = playerIds.Except(players.Select(p => p.Id)).ToList();
            if (missing.Any())
                throw new InvalidOperationException($"Играч(и) с id [{string.Join(", ", missing)}] не бяха намерени в базата.");
        }

        var trainingSession = new TrainingSession
        {
            GameSaveId = gameSaveId,
            TeamId = teamId,
            SeasonId = seasonId,
            Date = date,
            PlayerTrainings = new List<PlayerTraining>()
        };

        var results = new List<TrainingResultDto>();

        foreach (var assignment in assignments)
        {
            var player = players.FirstOrDefault(p => p.Id == assignment.PlayerId);
            if (player == null)
            {
                _logger?.LogError("❌ Играч {Id} липсва след зареждане", assignment.PlayerId);
                continue; 
            }

            var pa = player.Attributes.FirstOrDefault(a => a.AttributeId == assignment.AttributeId);
            if (pa == null)
            {
                _logger?.LogError("❌ Играч {Id} няма атрибут {AttrId}", player.Id, assignment.AttributeId);
                continue; 
            }

            if (double.IsNaN(pa.Progress))
                pa.Progress = 0;

            var oldValue = pa.Value;

            double baseGain = 0.03;
            double ageFactor = player.Age < 21 ? 1.3 : player.Age > 28 ? 0.7 : 1.0;
            double randomFactor = _random.NextDouble() * 0.3 + 0.85;
            double gain = baseGain * ageFactor * randomFactor;

            pa.Progress += gain;

            int changeValue = 0;
            if (pa.Progress >= 1.0)
            {
                pa.Progress -= 1.0;
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
                ProgressGain = Math.Round(gain, 3),
                TotalProgress = Math.Round(pa.Progress, 3)
            });
        }

        _context.TrainingSessions.Add(trainingSession);
        await _context.SaveChangesAsync();

        return results;
    }

}
