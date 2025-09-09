namespace TheDugout.Models
{
    public class TrainingSession
    {
        public int Id { get; set; }

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public DateTime Date { get; set; }   

        public ICollection<PlayerTraining> PlayerTrainings { get; set; } = new List<PlayerTraining>();
    }
}
