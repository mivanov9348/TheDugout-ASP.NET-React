namespace TheDugout.Models
{
    public class Season
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; }
        public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 7, 1);
        public DateTime EndDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public ICollection<SeasonEvent> Events { get; set; } = new List<SeasonEvent>();
        public ICollection<PlayerSeasonStats> PlayerStats { get; set; } = new List<PlayerSeasonStats>();
    }
}
