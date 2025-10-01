namespace TheDugout.Services.Game
{
    public interface IGameDayService
    {
        Task ProcessNextDayAsync(int saveId, Func<string, Task>? progress = null);
        Task<object> ProcessNextDayAndGetResultAsync(int saveId);
    }
}
