namespace TheDugout.DTOs.DtoNewGame
{
    public class NewGameResponse
    {
        public int GameSaveId { get; set; }
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int SeasonId { get; set; }
        public DateTime SeasonStart { get; set; }
        public DateTime SeasonEnd { get; set; }
    }
}
