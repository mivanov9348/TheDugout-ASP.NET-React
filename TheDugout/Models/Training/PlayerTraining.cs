using TheDugout.Models.Players;

namespace TheDugout.Models.Training
{
    public class PlayerTraining
    {
        public int Id { get; set; }
        public int TrainingSessionId { get; set; }
        public TrainingSession TrainingSession { get; set; } = null!;
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int AttributeId { get; set; }
        public Players.Attribute Attribute { get; set; } = null!;
        public int ChangeValue { get; set; }   
    }
}
