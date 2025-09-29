namespace TheDugout.Services.Game
{
    public interface IGameDayService
    {
        Task ProcessNextDayAsync(int saveId);
        Task<object> ProcessNextDayAndGetResultAsync(int saveId);
    }
}
