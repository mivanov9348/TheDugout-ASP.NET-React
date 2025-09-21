namespace TheDugout.Services.Game
{
    public interface IGameDayService
    {
        void ProcessNextDayAsync(int saveId);

    }
}
