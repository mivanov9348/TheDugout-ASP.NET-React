namespace TheDugout.Services.Season
{
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Fixtures;
    using TheDugout.Services.Season.Interfaces;
    public class CupScheduleService : ICupScheduleService
    {
        public CupScheduleService()
        {
        }

        public void AssignCupFixtures(List<Fixture> fixtures, Season season)
        {
            var candidateDates = season.Events
                .Where(e => e.Type == SeasonEventType.CupMatch)
                .OrderBy(e => e.Date)
                .ToList();

            if (!candidateDates.Any())
                throw new InvalidOperationException("No dates available for cup matches.");

            // групираме по рунд (RoundNumber)
            var groupedByRound = fixtures.GroupBy(f => f.Round).OrderBy(g => g.Key);

            foreach (var roundFixtures in groupedByRound)
            {
                var roundIndex = roundFixtures.Key - 1;
                var date = candidateDates[Math.Min(roundIndex, candidateDates.Count - 1)];

                foreach (var fixture in roundFixtures)
                    fixture.Date = date.Date;
            }
        }
    }
}
