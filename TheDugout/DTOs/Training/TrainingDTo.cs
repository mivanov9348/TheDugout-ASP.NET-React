﻿public class TrainingRequestDto
{
    public int GameSaveId { get; set; }
    public int TeamId { get; set; }
    public int SeasonId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public List<PlayerTrainingAssignmentDto> Assignments { get; set; } = new();
}

public class PlayerTrainingAssignmentDto
{
    public int PlayerId { get; set; }
    public int AttributeId { get; set; }
}

public class TrainingResultDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public string AttributeName { get; set; } = "";
    public int OldValue { get; set; }
    public int NewValue { get; set; }
    public double ProgressGain { get; set; }
}