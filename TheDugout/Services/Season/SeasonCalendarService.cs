using TheDugout.Models.Seasons;

namespace TheDugout.Services.Season
{
    public class SeasonCalendarService : ISeasonCalendarService
    {
        public SeasonCalendarService()
        {
        }
        public DateTime GetNextFreeDate(Models.Seasons.Season season, SeasonEventType type, DateTime fromDate)
        {
            return season.Events
                .Where(e => !e.IsOccupied && e.Type == type && e.Date >= fromDate)
                .OrderBy(e => e.Date)
                .Select(e => e.Date)
                .FirstOrDefault();
        }

        // ---------- SPECIALIZED DISTRIBUTIONS ----------
        // Leagues
        public List<DateTime> DistributeLeagueRounds(Models.Seasons.Season season, int totalRounds)
            => DistributeGeneric(season, SeasonEventType.ChampionshipMatch, totalRounds, interval: 1);

        // Cups
        public List<DateTime> DistributeCupRounds(Models.Seasons.Season season, int totalRounds)
            => DistributeGeneric(season, SeasonEventType.CupMatch, totalRounds, interval: 1);

        // Eurocups
        public List<DateTime> DistributeEuropeanRounds(Models.Seasons.Season season, int totalRounds)
            => DistributeGeneric(season, SeasonEventType.EuropeanMatch, totalRounds, interval: 2);

        public List<DateTime> DistributeRounds(Models.Seasons.Season season, SeasonEventType type, int totalRounds, int interval = 1)
            => DistributeGeneric(season, type, totalRounds, interval);

        // ---------- CORE IMPLEMENTATION ----------
        private List<DateTime> DistributeGeneric(Models.Seasons.Season season, SeasonEventType type, int totalRounds, int interval = 1)
        {
            var candidateDates = season.Events
                .Where(e => e.Type == type && !e.IsOccupied)
                .OrderBy(e => e.Date)
                .Select(e => e.Date)
                .ToList();

            if (!candidateDates.Any())
                throw new InvalidOperationException($"No dates available for {type}");

            var distributed = new List<DateTime>();
            int index = 0;

            for (int round = 0; round < totalRounds; round++)
            {
                if (index >= candidateDates.Count)
                {
                    // fallback: генерираме нова дата след последната, през "interval" седмици
                    distributed.Add(distributed.Last().AddDays(interval * 7));
                }
                else
                {
                    distributed.Add(candidateDates[index]);
                    index += interval;
                }
            }

            return distributed;
        }
    }
}
