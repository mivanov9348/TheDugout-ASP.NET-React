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
            _logger.LogInformation("🚀 Стартира масова тренировка за ВСИЧКИ отбори - GameSaveId: {GameSaveId}, Дата: {Date}", gameSaveId, universalDate);

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                // 1️⃣ Зареждаме вече тренирали отбори
                var trainedTeamIds = await _context.TrainingSessions
                    .Where(ts => ts.GameSaveId == gameSaveId && ts.Date == universalDate)
                    .Select(ts => ts.TeamId)
                    .ToHashSetAsync();

                // 2️⃣ Зареждаме всички отбори
                var allTeams = await _context.Teams
                    .Where(t => t.GameSaveId == gameSaveId)
                    .Select(t => t.Id)
                    .ToListAsync();

                if (!allTeams.Any())
                {
                    _logger.LogWarning("⚠️ Няма нито един отбор в GameSave {GameSaveId}!", gameSaveId);
                    return;
                }

                // 3️⃣ Зареждаме позиционните тежести
                var weights = await _context.PositionWeights
                    .AsNoTracking()
                    .ToDictionaryAsync(pw => (pw.PositionId, pw.AttributeId), pw => pw.Weight);

                // 4️⃣ Зареждаме всички дефинирани планове (TeamTrainingPlan)
                var teamPlans = await _context.TeamTrainingPlans
                    .Where(tp => tp.GameSaveId == gameSaveId)
                    .GroupBy(tp => tp.TeamId)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                // 5️⃣ Зареждаме всички играчи
                var players = await _context.Players
                    .AsNoTracking()
                    .Where(p => p.GameSaveId == gameSaveId && p.TeamId.HasValue)
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

                if (players.Count == 0)
                {
                    _logger.LogInformation("✅ Няма играчи за трениране в този GameSave.");
                    return;
                }

                var teamGroups = players.GroupBy(p => p.TeamId!.Value).ToList();
                var allTrainingSessions = new List<TrainingSession>();
                var random = new Random();
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2) };

                _logger.LogInformation("🏋️‍♂️ Стартира паралелна тренировка ({Threads} нишки)...", parallelOptions.MaxDegreeOfParallelism);

                Parallel.ForEach(teamGroups, parallelOptions, teamGroup =>
                {
                    try
                    {
                        var teamId = teamGroup.Key;
                        var hasPlan = teamPlans.ContainsKey(teamId);
                        var planForTeam = hasPlan ? teamPlans[teamId] : null;

                        var trainingSession = new TrainingSession
                        {
                            GameSaveId = gameSaveId,
                            TeamId = teamId,
                            SeasonId = seasonId,
                            Date = universalDate,
                            PlayerTrainings = new List<PlayerTraining>()
                        };

                        foreach (var player in teamGroup)
                        {
                            int? trainedAttrId = null;

                            // 🧠 Ако има зададен план за отбора
                            if (hasPlan)
                            {
                                var playerPlan = planForTeam!.FirstOrDefault(tp => tp.PlayerId == player.Id);
                                trainedAttrId = playerPlan?.AttributeId;
                            }

                            // Ако няма индивидуален план, ползвай автоматичното
                            if (trainedAttrId == null)
                            {
                                double bestScore = double.MinValue;
                                foreach (var pa in player.Attributes)
                                {
                                    if (!player.PositionId.HasValue) continue;
                                    if (!weights.TryGetValue((player.PositionId.Value, pa.AttributeId), out var weight))
                                        continue;

                                    var bonus = 1.0 / (pa.Value + 1);
                                    var score = weight + bonus;
                                    if (score > bestScore)
                                    {
                                        bestScore = score;
                                        trainedAttrId = pa.AttributeId;
                                    }
                                }
                            }

                            if (trainedAttrId == null) continue;

                            double baseGain = 0.03;
                            double ageFactor = player.Age < 21 ? 1.3 : player.Age > 28 ? 0.7 : 1.0;
                            double randomFactor = random.NextDouble() * 0.3 + 0.85;
                            double gain = baseGain * ageFactor * randomFactor;

                            var paData = player.Attributes.First(a => a.AttributeId == trainedAttrId);
                            double progress = paData.Progress + gain;
                            int changeValue = 0;

                            if (progress >= 1.0)
                            {
                                progress -= 1.0;
                                changeValue = 1;
                            }

                            trainingSession.PlayerTrainings.Add(new PlayerTraining
                            {
                                PlayerId = player.Id,
                                AttributeId = trainedAttrId.Value,
                                GameSaveId = gameSaveId,
                                ChangeValue = changeValue
                            });
                        }

                        lock (allTrainingSessions)
                        {
                            if (trainingSession.PlayerTrainings.Count > 0)
                                allTrainingSessions.Add(trainingSession);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Грешка при обработка на отбор {TeamId}", teamGroup.Key);
                    }
                });

                // 6️⃣ Масово записване
                if (allTrainingSessions.Count > 0)
                {
                    await _context.BulkInsertAsync(allTrainingSessions);
                    await _context.BulkInsertAsync(allTrainingSessions.SelectMany(s => s.PlayerTrainings));

                    _logger.LogInformation("✅ Успешна тренировка: {Teams} отбора, {Trainings} тренировки.",
                        allTrainingSessions.Count,
                        allTrainingSessions.Sum(t => t.PlayerTrainings.Count));
                }

                // 7️⃣ Обновяване на атрибутите
                var updatedAttributes = allTrainingSessions
                    .SelectMany(s => s.PlayerTrainings)
                    .GroupBy(pt => new { pt.PlayerId, pt.AttributeId })
                    .Select(g => new
                    {
                        g.Key.PlayerId,
                        g.Key.AttributeId,
                        Gain = g.Sum(pt => pt.ChangeValue)
                    })
                    .ToList();

                foreach (var ua in updatedAttributes)
                {
                    var attr = await _context.PlayerAttributes
                        .FirstOrDefaultAsync(a => a.PlayerId == ua.PlayerId && a.AttributeId == ua.AttributeId && a.GameSaveId == gameSaveId);

                    if (attr != null)
                    {
                        attr.Value += ua.Gain;
                        attr.Progress = Math.Min(1, attr.Progress + 0.03);
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
                    if (pa.Value < 20) 
                    {
                        pa.Value += 1;
                        changeValue = 1;
                    }
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

        public async Task SaveTrainingAsync(TrainingRequestDto request)
        {
            if (request == null || request.Assignments == null || !request.Assignments.Any())
                throw new ArgumentException("Invalid or empty training data.");

            _logger.LogInformation("📘 SaveTraining: GameSaveId={GameSaveId}, TeamId={TeamId}, {Count} assignments",
                request.GameSaveId, request.TeamId, request.Assignments.Count);

            var teamId = request.TeamId;
            var gameSaveId = request.GameSaveId;

            var activeSeason = await _context.Seasons
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (activeSeason == null)
            {
                throw new InvalidOperationException("Active season not found for the given GameSaveId.");
            }

            // 1️⃣ Изтриваме старите записи за този отбор
            var existingPlans = _context.TeamTrainingPlans
                .Where(p => p.GameSaveId == gameSaveId && p.TeamId == teamId);

            _context.TeamTrainingPlans.RemoveRange(existingPlans);
            await _context.SaveChangesAsync();

            // 2️⃣ Добавяме новите записи
            var newPlans = request.Assignments.Select(a => new TeamTrainingPlan
            {
                GameSaveId = gameSaveId,
                TeamId = teamId,
                PlayerId = a.PlayerId,
                AttributeId = a.AttributeId,
                AssignedAt = activeSeason.CurrentDate
            }).ToList();

            await _context.TeamTrainingPlans.AddRangeAsync(newPlans);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Saved {Count} training plans for team {TeamId}.", newPlans.Count, teamId);
        }

    }
}