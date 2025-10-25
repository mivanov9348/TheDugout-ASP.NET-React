namespace TheDugout.Services.Season
{
    using TheDugout.Models.Seasons;
    using TheDugout.Services.Season.Interfaces;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Enums;

    public class LeagueScheduleService : ILeagueScheduleService
    {
        public LeagueScheduleService()
        {
        }

        public void AssignLeagueFixtures(List<Fixture> fixtures, Season season)
        {
            var totalRounds = fixtures.Max(f => f.Round);

            // 1. Взимаме САМО ОСНОВНИТЕ дни за мачове (Съботите)
            // Така 'step' логиката ще работи правилно,
            // като намира по един уикенд за всеки кръг.
            var primaryMatchDays = season.Events
                .Where(e => e.Type == SeasonEventType.ChampionshipMatch && e.Date.DayOfWeek == DayOfWeek.Saturday)
                .OrderBy(e => e.Date)
                .ToList();

            if (!primaryMatchDays.Any())
                throw new InvalidOperationException("No primary league match days (Saturdays) found in season events.");

            // 'step' разпределя кръговете равномерно спрямо наличните СЪБОТИ
            double step = (double)primaryMatchDays.Count / totalRounds;

            for (int round = 1; round <= totalRounds; round++)
            {
                // 2. Намираме датата за СЪБОТА за този кръг
                int idx = (int)Math.Floor((round - 1) * step);
                if (idx >= primaryMatchDays.Count)
                    idx = primaryMatchDays.Count - 1;

                var saturdayDate = primaryMatchDays[idx].Date;

                // 3. Взимаме всички мачове за кръга
                var fixturesForRound = fixtures.Where(f => f.Round == round).ToList();
                if (!fixturesForRound.Any()) continue;

                // 4. Проверяваме дали НЕДЕЛЯ е свободна и маркирана за мачове
                var potentialSundayDate = saturdayDate.AddDays(1);
                var sundayEvent = season.Events
                                    .FirstOrDefault(e => e.Date.Date == potentialSundayDate.Date);

                // Неделя е налична, АКО съществува event на тази дата И той е
                // от тип ChampionshipMatch (който ние зададохме в GetEventType)
                bool isSundayAvailable = sundayEvent != null &&
                                         sundayEvent.Type == SeasonEventType.ChampionshipMatch;

                if (isSundayAvailable)
                {
                    // 5. УСПЕХ: Разделяме кръга на две (Събота / Неделя)

                    // Math.Ceiling гарантира, че нечетен брой мачове
                    // (напр. 5) ще се раздели на 3 (събота) и 2 (неделя).
                    int halfCount = (int)Math.Ceiling(fixturesForRound.Count / 2.0);

                    var saturdayFixtures = fixturesForRound.Take(halfCount);
                    var sundayFixtures = fixturesForRound.Skip(halfCount);

                    // Присвояваме датите
                    foreach (var fixture in saturdayFixtures)
                    {
                        fixture.Date = saturdayDate;
                    }

                    foreach (var fixture in sundayFixtures)
                    {
                        fixture.Date = potentialSundayDate;
                    }
                }
                else
                {
                    // 6. FALLBACK: Неделя е заета (напр. финал за купа)
                    // Слагаме всички мачове в събота, както беше оригинално.
                    foreach (var fixture in fixturesForRound)
                    {
                        fixture.Date = saturdayDate;
                    }
                }
            }
        }
    }
}

