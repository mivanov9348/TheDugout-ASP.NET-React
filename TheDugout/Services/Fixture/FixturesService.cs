using Microsoft.EntityFrameworkCore;
using TheDugout.Data;

namespace TheDugout.Services.Fixture
{
    public class FixturesService : IFixturesService
    {
        private readonly DugoutDbContext _context;


        public FixturesService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task GenerateFixturesAsync(int gameSaveId, int seasonId, DateTime startDate)
        {
            var leagues = await _context.Leagues
                .Include(l => l.Teams)
                .Where(l => l.GameSaveId == gameSaveId)
                .ToListAsync();

            foreach (var league in leagues)
            {
                if (!league.Teams.Any() || league.Teams.Count < 2)
                    continue;

                var teams = league.Teams.ToList();
                if (teams.Count % 2 != 0)
                    teams.Add(null!); 

                int rounds = teams.Count - 1;
                int matchesPerRound = teams.Count / 2;

                var fixtures = new List<Models.Fixture>();
                DateTime currentDate = startDate;
                int roundNumber = 1;

                // first half of the season
                for (int round = 0; round < rounds; round++)
                {
                    for (int match = 0; match < matchesPerRound; match++)
                    {
                        var home = teams[match];
                        var away = teams[teams.Count - 1 - match];

                        if (home != null && away != null)
                        {
                            fixtures.Add(new Models.Fixture
                            {
                                GameSaveId = gameSaveId,
                                LeagueId = league.Id,
                                SeasonId = seasonId,
                                HomeTeamId = home.Id,
                                AwayTeamId = away.Id,
                                Date = currentDate,
                                Round = roundNumber
                            });
                        }
                    }

                    // rotate teams 
                    var last = teams[teams.Count - 1];
                    teams.RemoveAt(teams.Count - 1);
                    teams.Insert(1, last);

                    currentDate = currentDate.AddDays(7); 
                    roundNumber++;
                }

                // second half of the season
                int firstLegRounds = roundNumber - 1;
                foreach (var f in fixtures.ToList()) 
                {
                    fixtures.Add(new Models.Fixture
                    {
                        GameSaveId = f.GameSaveId,
                        LeagueId = f.LeagueId,
                        SeasonId = f.SeasonId,
                        HomeTeamId = f.AwayTeamId,
                        AwayTeamId = f.HomeTeamId,
                        Date = currentDate,
                        Round = f.Round + firstLegRounds
                    });
                    currentDate = currentDate.AddDays(7);
                }

                await _context.Fixtures.AddRangeAsync(fixtures);
            }

            await _context.SaveChangesAsync();
        }
    }
}