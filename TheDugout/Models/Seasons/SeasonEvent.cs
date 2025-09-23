namespace TheDugout.Models.Seasons
{

    public enum SeasonEventType
    {
        ChampionshipMatch,
        CupMatch,
        EuropeanMatch,
        FriendlyMatch,
        TransferWindow,
        TrainingDay,
        Other
    }
    public class SeasonEvent
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public DateTime Date { get; set; }
        public SeasonEventType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsOccupied { get; set; } = false;

    }
}
