namespace TheDugout.Services.Training
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Training;
    using TheDugout.Services.Training;
    public class TrainingService : ITrainingService
    {
        private readonly DugoutDbContext _context;
        private readonly ILogger<TrainingService> _logger;
        private readonly Random _random = new Random();

        public TrainingService(DugoutDbContext context, ILogger<TrainingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================================================================
        // 1. ОПТИМИЗИРАН МЕТОД ЗА МАСОВА ТРЕНИРОВКА НА ВСИЧКИ CPU ОТБОРИ
        // ==========================================================================================
        public async Task RunDailyTrainingForAllCpuTeamsAsync(int gameSaveId, int seasonId, DateTime date, int? humanTeamId)
        {
            var universalDate = date.ToUniversalTime().Date;
            _logger.LogInformation("🚀 Стартира масова тренировка за GameSaveId: {GameSaveId}, Дата: {Date}", gameSaveId, universalDate);

            try
            {
                // СТЪПКА 1: Вземаме ID-тата на отборите, които вече са тренирали днес.
                var trainedTeamIds = await _context.TrainingSessions
                    .Where(ts => ts.GameSaveId == gameSaveId && ts.Date == universalDate)
                    .Select(ts => ts.TeamId)
                    .ToHashSetAsync();

                // СТЪПКА 2: Вземаме ВСИЧКИ играчи, които трябва да тренират, с филтър за отбора на човека.
                var playersToTrain = await _context.Players
                    .Include(p => p.Attributes)
                        .ThenInclude(pa => pa.Attribute)
                            .ThenInclude(a => a.PositionWeights)
                    .Where(p =>
                        p.GameSaveId == gameSaveId &&
                        p.TeamId.HasValue && // Важно: филтрираме само играчи с отбор
                        !trainedTeamIds.Contains(p.TeamId.Value) &&
                        (humanTeamId == null || p.TeamId != humanTeamId.Value))
                    .ToListAsync();

                if (!playersToTrain.Any())
                {
                    _logger.LogInformation("✅ Всички отбори вече са тренирали за деня. Няма какво да се прави.");
                    return;
                }

                _logger.LogInformation("Намерени {PlayerCount} играчи от {TeamCount} отбора за тренировка.",
                    playersToTrain.Count,
                    playersToTrain.Select(p => p.TeamId).Distinct().Count());

                var newTrainingSessions = new Dictionary<int, TrainingSession>();

                // СТЪПКА 3: Изпълняваме цялата логика в паметта.
                foreach (var player in playersToTrain)
                {
                    // Пропускаме играчи без отбор (за подсигуряване)
                    if (!player.TeamId.HasValue) continue;

                    int teamId = player.TeamId.Value; // Взимаме non-null стойността

                    // Стъпка 3А: Автоматично избиране на най-добрия атрибут за тренировка
                    var positionWeightsMap = player.Attributes
                        .SelectMany(pa => pa.Attribute.PositionWeights)
                        .Where(pw => pw.PositionId == player.PositionId)
                        .ToDictionary(pw => pw.AttributeId, pw => pw.Weight);

                    var bestAttr = player.Attributes
                        .OrderByDescending(pa =>
                        {
                            positionWeightsMap.TryGetValue(pa.AttributeId, out var weight);
                            var bonusForLowValue = 1.0 / (pa.Value + 1);
                            return weight + bonusForLowValue;
                        })
                        .FirstOrDefault();

                    if (bestAttr == null) continue;

                    // Стъпка 3Б: Симулиране на тренировката и изчисляване на прогреса
                    if (double.IsNaN(bestAttr.Progress)) bestAttr.Progress = 0;

                    double baseGain = 0.03;
                    double ageFactor = player.Age < 21 ? 1.3 : player.Age > 28 ? 0.7 : 1.0;
                    double randomFactor = _random.NextDouble() * 0.3 + 0.85;
                    double gain = baseGain * ageFactor * randomFactor;

                    bestAttr.Progress += gain;

                    int changeValue = 0;
                    if (bestAttr.Progress >= 1.0)
                    {
                        bestAttr.Progress -= 1.0;
                        bestAttr.Value += 1;
                        changeValue = 1;
                    }

                    // Групиране на резултатите по отбори (сега използваме non-null teamId)
                    if (!newTrainingSessions.ContainsKey(teamId))
                    {
                        newTrainingSessions[teamId] = new TrainingSession
                        {
                            GameSaveId = gameSaveId,
                            TeamId = teamId, 
                            SeasonId = seasonId,
                            Date = universalDate,
                            PlayerTrainings = new List<PlayerTraining>()
                        };
                    }

                    newTrainingSessions[teamId].PlayerTrainings.Add(new PlayerTraining
                    {
                        PlayerId = player.Id,
                        AttributeId = bestAttr.AttributeId,
                        GameSaveId = gameSaveId,
                        ChangeValue = changeValue
                    });
                }

                // СТЪПКА 4: Записваме ВСИЧКИ промени в базата с ЕДНА транзакция.
                if (newTrainingSessions.Any())
                {
                    _context.TrainingSessions.AddRange(newTrainingSessions.Values);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("✅ Успешно проведена и записана тренировка за {Count} отбора.", newTrainingSessions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Критична грешка по време на масовата тренировка за GameSaveId: {GameSaveId}", gameSaveId);
                throw;
            }
        }

        // ==========================================================================================
        // 2. МЕТОДИ ЗА ОПЕРАЦИИ С ЕДИН ОТБОР (ПОДХОДЯЩИ ЗА UI)
        // ==========================================================================================

        public async Task<List<AutoAssignResultDto>> AutoAssignAttributesAsync(int teamId, int gameSaveId)
        {
            var players = await _context.Players
                .AsNoTracking()
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

        public async Task<List<TrainingResultDto>> RunTrainingSessionAsync(int gameSaveId, int teamId, int seasonId, DateTime date, List<PlayerTrainingAssignmentDto> assignments)
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
                if (player == null) continue;

                var pa = player.Attributes.FirstOrDefault(a => a.AttributeId == assignment.AttributeId);
                if (pa == null) continue;

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
}