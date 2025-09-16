using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Matches;
using TheDugout.Models.Seasons;
using TheDugout.Services.Season;

namespace TheDugout.Services.Fixture
{
    public class CupFixturesService : ICupFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixtureHelperService;
        private readonly ISeasonCalendarService _seasonCalendarService;
        private readonly Random _random = new Random();
        private static readonly ConcurrentDictionary<int, DateTime> _globalCupStartDates = new();

        public CupFixturesService(
            DugoutDbContext context,
            IFixturesHelperService fixtureHelperService,
            ISeasonCalendarService seasonCalendarService)
        {
            _context = context;
            _fixtureHelperService = fixtureHelperService;
            _seasonCalendarService = seasonCalendarService;
        }

        public async Task GenerateCupFixturesAsync(Models.Competitions.Cup cup, int seasonId, int gameSaveId, bool shareCupStartDate = true, bool markSeasonEvent = false)
        {
            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            if (season == null) return;

            var teams = cup.Teams
                .Select(ct => new TheDugout.Models.Teams.Team
                {
                    Id = ct.TeamId,
                    Name = ct.Team?.Name ?? $"Team {ct.TeamId}"
                })
                .ToList();

            if (teams.Count < 2) return;

            // shuffle teams
            teams = teams.OrderBy(_ => _random.Next()).ToList();

            var prevPow = (int)Math.Pow(2, Math.Floor(Math.Log(teams.Count, 2)));
            int prelimMatches = Math.Max(0, teams.Count - prevPow);

            DateTime cupDate;
            if (shareCupStartDate)
            {
                // get-or-add in thread-safe way per season
                cupDate = _globalCupStartDates.GetOrAdd(seasonId, sid =>
                {
                    var d = _seasonCalendarService.GetNextFreeDate(season, SeasonEventType.CupMatch, season.StartDate);
                    if (d == default(DateTime) || d == DateTime.MinValue)
                        d = DateTime.UtcNow.AddDays(7);
                    return d;
                });
            }
            else
            {
                var d = _seasonCalendarService.GetNextFreeDate(season, SeasonEventType.CupMatch, season.StartDate);
                cupDate = (d == default(DateTime) || d == DateTime.MinValue) ? DateTime.UtcNow.AddDays(7) : d;
            }

            // find a season event (match by Date only to avoid time-of-day mismatch)
            var matchingEvent = season.Events
                .FirstOrDefault(e => e.Type == SeasonEventType.CupMatch && e.Date.Date == cupDate.Date);

            // ---------- PRELIM ----------
            if (prelimMatches > 0)
            {
                var prelimRound = new CupRound
                {
                    CupId = cup.Id,
                    RoundNumber = 1,
                    Name = "Preliminary Round"
                };

                // take exactly 2 * prelimMatches teams for prelim
                var prelimTeams = teams.Take(prelimMatches * 2).ToList();

                for (int i = 0; i < prelimMatches; i++)
                {
                    var home = prelimTeams[i * 2];
                    var away = prelimTeams[i * 2 + 1];

                    prelimRound.Fixtures.Add(_fixtureHelperService.CreateFixture(
                        gameSaveId,
                        seasonId,
                        home.Id,
                        away.Id,
                        cupDate,
                        1,
                        CompetitionType.DomesticCup,
                        prelimRound
                    ));
                }

                _context.CupRounds.Add(prelimRound);

                if (matchingEvent != null && markSeasonEvent)
                    matchingEvent.IsOccupied = true;

                await _context.SaveChangesAsync();
                return; // генерираме само предварителния рунд за сега
            }

            // ---------- ROUND 1 ----------
            // safety: make teams even (should already be true if prevPow logic said no prelim)
            if (teams.Count % 2 != 0)
                teams = teams.Take(teams.Count - 1).ToList();

            var round1 = new CupRound
            {
                CupId = cup.Id,
                RoundNumber = 1,
                Name = "Round 1"
            };

            for (int i = 0; i < teams.Count; i += 2)
            {
                var home = teams[i];
                var away = teams[i + 1];

                round1.Fixtures.Add(_fixtureHelperService.CreateFixture(
                    gameSaveId,
                    seasonId,
                    home.Id,
                    away.Id,
                    cupDate,
                    1,
                    CompetitionType.DomesticCup,
                    round1
                ));
            }

            _context.CupRounds.Add(round1);

            if (matchingEvent != null && markSeasonEvent)
                matchingEvent.IsOccupied = true;

            await _context.SaveChangesAsync();
        }
    }

}
