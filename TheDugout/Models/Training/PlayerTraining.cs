namespace TheDugout.Models.Training
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;

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
        public int? SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public int ChangeValue { get; set; }   
    }
}
