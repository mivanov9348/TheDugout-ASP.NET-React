namespace TheDugout.Models.Training
{
    public class TeamTrainingPlan
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public int TeamId { get; set; }
        public int PlayerId { get; set; }
        public int AttributeId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
