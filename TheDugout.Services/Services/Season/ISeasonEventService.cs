namespace TheDugout.Services.Season
{
    public interface ISeasonEventService
    {
        Task CheckEventsForDayAsync(int seasonId, DateTime date);

    }
}
