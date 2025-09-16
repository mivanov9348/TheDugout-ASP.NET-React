namespace TheDugout.Services.Season
{
    public interface ISeasonSchedulingService
    {
        void AssignFixtureDates(
   List<Models.Matches.Fixture> fixtures,
   Models.Seasons.Season season,
   DateTime fallbackStartDate);
    }
}

