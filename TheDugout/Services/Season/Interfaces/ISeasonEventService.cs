namespace TheDugout.Services.Season.Interfaces
{
    public interface ISeasonEventService
    {
        Task CheckEventsForDayAsync(int seasonId, DateTime date);

    }
}
