namespace TheDugout.Services.Game.Interfaces
{
    public interface IGameDayService
    {
        Task ProcessNextDayAsync(int saveId, Func<string, Task>? progress = null);
        Task<object> ProcessNextDayAndGetResultAsync(int saveId);
    }
}
