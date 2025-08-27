namespace TheDugout.Data.DtoNewGame
{
    public class NewGameResponse
    {
        public int GameSaveId { get; set; }
        public int UserTeamId { get; set; }
        public string UserTeamName { get; set; } = null!;
    }
}
