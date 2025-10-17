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
                .Where(e => e.Type == SeasonEventType.CupMatch && !e.IsOccupied)
                .OrderBy(e => e.Date)
                .ToList();

            if (!candidateDates.Any())
                throw new InvalidOperationException("No dates available for cup matches.");

            var groupedByRound = fixtures.GroupBy(f => f.Round).OrderBy(g => g.Key);
            var dateQueue = new Queue<SeasonEvent>(candidateDates);

            foreach (var roundFixtures in groupedByRound)
            {
                if (dateQueue.Count == 0)
                    throw new InvalidOperationException("Not enough free dates for cup matches.");

                var nextDate = dateQueue.Dequeue();

                foreach (var fixture in roundFixtures)
                    fixture.Date = nextDate.Date;

                nextDate.IsOccupied = true;
            }
        }
    }
}
