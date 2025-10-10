namespace TheDugout.Models.Training
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    public class PlayerTraining
    {
        public int Id { get; set; }
        public int? TrainingSessionId { get; set; }
        public TrainingSession TrainingSession { get; set; } = null!;
        public int? PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int? AttributeId { get; set; }
        public AttributeDefinition Attribute { get; set; } = null!;
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int ChangeValue { get; set; }   
    }
}
