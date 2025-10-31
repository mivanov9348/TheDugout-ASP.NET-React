namespace TheDugout.Services.Training
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Training;
    using TheDugout.Services.Training.Interfaces;
    using EFCore.BulkExtensions;

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

        public async Task RunDailyTrainingForAllCpuTeamsAsync(int gameSaveId, int seasonId, DateTime date, int? humanTeamId)
        {
            var universalDate = date.ToUniversalTime().Date;
            _logger.LogInformation("🚀 Стартира масова тренировка за GameSaveId: {GameSaveId}, Дата: {Date}", gameSaveId, universalDate);

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                // ⚡ 1. Вземаме ID-та на отборите, които вече са тренирали
                _logger.LogDebug("📦 Зареждане на вече тренирали отбори...");
                var trainedTeamIds = await _context.TrainingSessions
                    .Where(ts => ts.GameSaveId == gameSaveId && ts.Date == universalDate)
                    .Select(ts => ts.TeamId)
                    .ToHashSetAsync();
                _logger.LogInformation("🔍 Открити вече тренирали отбори: {Count}", trainedTeamIds.Count);

                // ⚡ 2. Кешираме всички PositionWeights за бърз достъп
                _logger.LogDebug("⚙️ Зареждане на PositionWeights...");
                var weights = await _context.PositionWeights
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        pw => (pw.PositionId, pw.AttributeId),
                        pw => pw.Weight
                    );
                _logger.LogInformation("📊 Заредени {Count} позиционни тежести.", weights.Count);

                // ⚡ 3. Зареждаме само нужните данни (без Include)
                _logger.LogDebug("👥 Зареждане на играчи за трениране...");
                var players = await _context.Players
                    .AsNoTracking()
                    .Where(p =>
                        p.GameSaveId == gameSaveId &&
                        p.TeamId.HasValue &&
                        !trainedTeamIds.Contains(p.TeamId.Value) &&
                        (humanTeamId == null || p.TeamId != humanTeamId.Value))
                    .Select(p => new
                    {
                        p.Id,
                        p.TeamId,
                        p.PositionId,
                        p.Age,
                        Attributes = p.Attributes.Select(a => new
                        {
                            a.AttributeId,
                            a.Value,
                            a.Progress
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("👟 Заредени общо {Count} играчи за CPU тренировка.", players.Count);

                if (players.Count == 0)
                {
                    _logger.LogInformation("✅ Няма CPU отбори за трениране днес.");
                    return;
                }

                var teamGroups = players.GroupBy(p => p.TeamId!.Value).ToList();
                _logger.LogInformation("📊 Ще тренират {TeamCount} CPU отбора ({PlayerCount} играчи)",
                    teamGroups.Count, players.Count);

                // ⚙️ 4. Подготвяме контейнера за масов insert
                var allTrainingSessions = new List<TrainingSession>();
                var random = new Random();

                // ⚡ 5. Паралелна обработка на отборите
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 };

                _logger.LogInformation("🏋️‍♂️ Стартира паралелна тренировка ({Threads} нишки)...", parallelOptions.MaxDegreeOfParallelism);

                Parallel.ForEach(teamGroups, parallelOptions, teamGroup =>
                {
                    try
                    {
                        var trainingSession = new TrainingSession
                        {
                            GameSaveId = gameSaveId,
                            TeamId = teamGroup.Key,
                            SeasonId = seasonId,
                            Date = universalDate,
                            PlayerTrainings = new List<PlayerTraining>()
                        };

                        foreach (var player in teamGroup)
                        {
                            double bestScore = double.MinValue;
                            int? bestAttrId = null;
                            double bestProgress = 0;
                            int bestValue = 0;

                            foreach (var pa in player.Attributes)
                            {
                                if (!player.PositionId.HasValue) continue;
                                if (!weights.TryGetValue((player.PositionId.Value, pa.AttributeId), out var weight))
                                    continue;

                                var bonusForLowValue = 1.0 / (pa.Value + 1);
                                var score = weight + bonusForLowValue;

                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    bestAttrId = pa.AttributeId;
                                    bestProgress = pa.Progress;
                                    bestValue = pa.Value;
                                }
                            }

                            if (bestAttrId == null) continue;
                            if (double.IsNaN(bestProgress)) bestProgress = 0;

                            double baseGain = 0.03;
                            double ageFactor = player.Age < 21 ? 1.3 : player.Age > 28 ? 0.7 : 1.0;
                            double randomFactor = random.NextDouble() * 0.3 + 0.85;
                            double gain = baseGain * ageFactor * randomFactor;

                            bestProgress += gain;

                            int changeValue = 0;
                            if (bestProgress >= 1.0)
                            {
                                bestProgress -= 1.0;
                                bestValue += 1;
                                changeValue = 1;
                            }

                            trainingSession.PlayerTrainings.Add(new PlayerTraining
                            {
                                PlayerId = player.Id,
                                AttributeId = bestAttrId.Value,
                                GameSaveId = gameSaveId,
                                ChangeValue = changeValue
                            });
                        }

                        lock (allTrainingSessions)
                        {
                            if (trainingSession.PlayerTrainings.Count > 0)
                            {
                                allTrainingSessions.Add(trainingSession);
                                _logger.LogDebug("✅ Отбор {TeamId} добавен ({Trainings} тренировки).",
                                    teamGroup.Key, trainingSession.PlayerTrainings.Count);
                            }
                            else
                            {
                                _logger.LogDebug("⚠️ Отбор {TeamId} няма тренировъчни данни.", teamGroup.Key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Грешка при обработка на отбор {TeamId}", teamGroup.Key);
                    }
                });

                // ⚡ 6. Масов insert (Bulk)
                if (allTrainingSessions.Count > 0)
                {
                    _logger.LogInformation("💾 Записване на {TeamCount} тренировки ({TotalTrainings} общо)...",
                        allTrainingSessions.Count,
                        allTrainingSessions.Sum(t => t.PlayerTrainings.Count));

                    await _context.BulkInsertAsync(allTrainingSessions);
                    await _context.BulkInsertAsync(allTrainingSessions.SelectMany(s => s.PlayerTrainings));

                    _logger.LogInformation("✅ Успешна тренировка: {Teams} отбора, {Trainings} тренировки.",
                        allTrainingSessions.Count,
                        allTrainingSessions.Sum(t => t.PlayerTrainings.Count));
                }
                else
                {
                    _logger.LogWarning("⚠️ Няма създадени тренировки за записване!");
                }

                var updatedAttributes = allTrainingSessions
                    .SelectMany(s => s.PlayerTrainings)
                    .GroupBy(pt => new { pt.PlayerId, pt.AttributeId })
                    .Select(g => new
                    {
                        g.Key.PlayerId,
                        g.Key.AttributeId,
                        Gain = g.Sum(pt => pt.ChangeValue) // може и да запишеш прогрес отделно
                    })
                    .ToList();

                foreach (var ua in updatedAttributes)
                {
                    var attr = await _context.PlayerAttributes
                        .FirstOrDefaultAsync(a => a.PlayerId == ua.PlayerId && a.AttributeId == ua.AttributeId && a.GameSaveId == gameSaveId);

                    if (attr != null)
                    {
                        attr.Value += ua.Gain; // +1 ако е минал прогрес
                        attr.Progress = Math.Min(1, attr.Progress + 0.03); // или друго число, според логиката
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("🎯 Обновени {Count} PlayerAttributes след тренировката.", updatedAttributes.Count);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Критична грешка по време на масовата тренировка за GameSaveId: {GameSaveId}", gameSaveId);
                throw;
            }
        }



        //public async Task RunDailyTrainingForAllCpuTeamsAsync(int gameSaveId, int seasonId, DateTime date, int? humanTeamId)
        //{
        //    var universalDate = date.ToUniversalTime().Date;
        //    _logger.LogInformation("🚀 Стартира масова тренировка за GameSaveId: {GameSaveId}, Дата: {Date}", gameSaveId, universalDate);

        //    try
        //    {
        //        var trainedTeamIds = await _context.TrainingSessions
        //            .Where(ts => ts.GameSaveId == gameSaveId && ts.Date == universalDate)
        //            .Select(ts => ts.TeamId)
        //            .ToHashSetAsync();

        //        var playersToTrain = await _context.Players
        //            .Include(p => p.Attributes)
        //                .ThenInclude(pa => pa.Attribute)
        //                    .ThenInclude(a => a.PositionWeights)
        //            .Where(p =>
        //                p.GameSaveId == gameSaveId &&
        //                p.TeamId.HasValue && 
        //                !trainedTeamIds.Contains(p.TeamId.Value) &&
        //                (humanTeamId == null || p.TeamId != humanTeamId.Value))
        //            .ToListAsync();

        //        if (!playersToTrain.Any())
        //        {
        //            _logger.LogInformation("✅ Всички отбори вече са тренирали за деня. Няма какво да се прави.");
        //            return;
        //        }

        //        _logger.LogInformation("Намерени {PlayerCount} играчи от {TeamCount} отбора за тренировка.",
        //            playersToTrain.Count,
        //            playersToTrain.Select(p => p.TeamId).Distinct().Count());

        //        var newTrainingSessions = new Dictionary<int, TrainingSession>();

        //        foreach (var player in playersToTrain)
        //        {
        //            if (!player.TeamId.HasValue) continue;

        //            int teamId = player.TeamId.Value; 

        //            var positionWeightsMap = player.Attributes
        //                .SelectMany(pa => pa.Attribute.PositionWeights)
        //                .Where(pw => pw.PositionId == player.PositionId)
        //                .ToDictionary(pw => pw.AttributeId, pw => pw.Weight);

        //            var bestAttr = player.Attributes
        //                .OrderByDescending(pa =>
        //                {
        //                    positionWeightsMap.TryGetValue(pa.AttributeId, out var weight);
        //                    var bonusForLowValue = 1.0 / (pa.Value + 1);
        //                    return weight + bonusForLowValue;
        //                })
        //                .FirstOrDefault();

        //            if (bestAttr == null) continue;

        //            if (double.IsNaN(bestAttr.Progress)) bestAttr.Progress = 0;

        //            double baseGain = 0.03;
        //            double ageFactor = player.Age < 21 ? 1.3 : player.Age > 28 ? 0.7 : 1.0;
        //            double randomFactor = _random.NextDouble() * 0.3 + 0.85;
        //            double gain = baseGain * ageFactor * randomFactor;

        //            bestAttr.Progress += gain;

        //            int changeValue = 0;
        //            if (bestAttr.Progress >= 1.0)
        //            {
        //                bestAttr.Progress -= 1.0;
        //                bestAttr.Value += 1;
        //                changeValue = 1;
        //            }

        //            if (!newTrainingSessions.ContainsKey(teamId))
        //            {
        //                newTrainingSessions[teamId] = new TrainingSession
        //                {
        //                    GameSaveId = gameSaveId,
        //                    TeamId = teamId,
        //                    SeasonId = seasonId,
        //                    Date = universalDate,
        //                    PlayerTrainings = new List<PlayerTraining>()
        //                };
        //            }

        //            newTrainingSessions[teamId].PlayerTrainings.Add(new PlayerTraining
        //            {
        //                PlayerId = player.Id,
        //                AttributeId = bestAttr.AttributeId,
        //                GameSaveId = gameSaveId,
        //                ChangeValue = changeValue
        //            });
        //        }

        //        if (newTrainingSessions.Any())
        //        {
        //            _context.TrainingSessions.AddRange(newTrainingSessions.Values);
        //            await _context.SaveChangesAsync();
        //            _logger.LogInformation("✅ Успешно проведена и записана тренировка за {Count} отбора.", newTrainingSessions.Count);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "❌ Критична грешка по време на масовата тренировка за GameSaveId: {GameSaveId}", gameSaveId);
        //        throw;
        //    }
        //}

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

        public async Task<List<TrainingResultDto>> RunTrainingSessionAsync(int gameSaveId, int teamId, List<PlayerTrainingAssignmentDto> assignments)
        {
            if (assignments == null || !assignments.Any())
                throw new InvalidOperationException("No assignments provided.");

            var season = _context.Seasons.FirstOrDefault(x => x.GameSaveId == gameSaveId && x.IsActive);

            if (season == null)
            {
                throw new InvalidOperationException("Season is null!");
            }

            var date = season.CurrentDate;

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
                SeasonId = season.Id,
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
                    SeasonId = season.Id,
                    GameSaveId = gameSaveId,
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