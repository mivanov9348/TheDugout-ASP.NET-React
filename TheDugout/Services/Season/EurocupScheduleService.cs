
using TheDugout.Models.Seasons;
using TheDugout.Services.Season.Interfaces;

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

        public List<DateTime> AssignKnockoutDatesAfter(Models.Seasons.Season season, DateTime lastGroupMatchDate, int remainingPhases)
        {
            var candidateDates = season.Events
                .Where(e => e.Type == SeasonEventType.EuropeanMatch
                            && !e.IsOccupied
                            && e.Date > lastGroupMatchDate)
                .OrderBy(e => e.Date)
                .ToList();

            if (!candidateDates.Any())
                throw new InvalidOperationException("No available EuropeanMatch dates after group stage.");

            int available = candidateDates.Count;
            if (remainingPhases <= 0)
                throw new InvalidOperationException("Invalid number of remaining phases.");

            List<DateTime> distributedDates = new();

            // Ако има по-малко дати от фази — взимаме всички налични
            if (available <= remainingPhases)
            {
                distributedDates = candidateDates
                    .Take(available)
                    .Select(e => e.Date)
                    .ToList();
            }
            else
            {
                // равномерно разпределяне по оста на датите
                double step = (double)(available - 1) / (remainingPhases - 1);

                HashSet<int> usedIndexes = new();

                for (int i = 0; i < remainingPhases; i++)
                {
                    // изчисляваме плаващ индекс
                    double floatIndex = i * step;
                    int index = (int)Math.Round(floatIndex);

                    // за всеки случай — избягваме дублиране при закръгляне
                    while (usedIndexes.Contains(index) && index < available - 1)
                        index++;

                    usedIndexes.Add(index);
                    distributedDates.Add(candidateDates[index].Date);
                }
            }

            // Отбелязваме избраните като заети
            foreach (var date in distributedDates)
            {
                var seasonEvent = candidateDates.FirstOrDefault(e => e.Date == date);
                if (seasonEvent != null)
                    seasonEvent.IsOccupied = true;
            }

            return distributedDates;
        }

    }
}
