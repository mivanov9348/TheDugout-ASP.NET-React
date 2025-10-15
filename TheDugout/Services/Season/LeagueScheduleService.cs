using TheDugout.Models.Seasons;
using TheDugout.Services.Season.Interfaces;

namespace TheDugout.Services.Season
{
    public class LeagueScheduleService : ILeagueScheduleService
    {
        public LeagueScheduleService()
        {
        }

        public void AssignLeagueFixtures(List<Models.Fixtures.Fixture> fixtures, Models.Seasons.Season season)
        {
            var totalRounds = fixtures.Max(f => f.Round);

            // Взимаме всички дати за шампионатни мачове
            var candidateDates = season.Events
                .Where(e => e.Type == SeasonEventType.ChampionshipMatch)
                .OrderBy(e => e.Date)
                .ToList();

            if (!candidateDates.Any())
                throw new InvalidOperationException("No dates available for league matches.");

            double step = (double)candidateDates.Count / totalRounds;

            for (int round = 1; round <= totalRounds; round++)
            {
                int idx = (int)Math.Floor((round - 1) * step);
                if (idx >= candidateDates.Count)
                    idx = candidateDates.Count - 1;

                var date = candidateDates[idx].Date;

                foreach (var fixture in fixtures.Where(f => f.Round == round))
                {
                    fixture.Date = date;
                }
            }
        }

    }
}

