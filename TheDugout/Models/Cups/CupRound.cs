namespace TheDugout.Models.Cups
{
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    public class CupRound
    {
        public int Id { get; set; }

        public int? CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int? RoundNumber { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
    }
}
