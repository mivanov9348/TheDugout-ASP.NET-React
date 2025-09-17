
using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public class EurocupScheduleService : IEurocupScheduleService
    {


        public List<DateTime> AssignEuropeanFixtures(Models.Seasons.Season season, int rounds)
        {
            var candidateDates = season.Events
                .Where(e => e.Type == SeasonEventType.EuropeanMatch
                            && !e.IsOccupied
                            && e.Date <= new DateTime(season.StartDate.Year, 12, 31))
                .OrderBy(e => e.Date)
                .ToList();

            if (!candidateDates.Any())
                throw new InvalidOperationException("No available EuropeanMatch dates in season calendar until end of year.");

            List<DateTime> distributedDates;

            if (candidateDates.Count <= rounds)
            {
                distributedDates = candidateDates
                    .Take(rounds)
                    .Select(e => e.Date)
                    .ToList();
            }
            else
            {
                distributedDates = new List<DateTime>();
                double step = (double)(candidateDates.Count - 1) / (rounds - 1);

                for (int i = 0; i < rounds; i++)
                {
                    int index = (int)Math.Round(i * step);
                    distributedDates.Add(candidateDates[index].Date);
                }
            }

            // отбелязваме използваните дати като заети
            foreach (var date in distributedDates)
            {
                var seasonEvent = candidateDates.FirstOrDefault(e => e.Date == date);
                if (seasonEvent != null) seasonEvent.IsOccupied = true;
            }

            return distributedDates;
        }
    }
}
