using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Teams;


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
                var teams = league.Teams.ToList();
                if (teams.Count < 2) continue;

                // If odd number of teams, add a dummy "BYE" team
                bool hasBye = teams.Count % 2 != 0;
                if (hasBye)
                {
                    teams.Add(new Models.Teams.Team { Id = -1, Name = "BYE" });
                }

                int teamCount = teams.Count;
                int rounds = teamCount - 1;          // each team plays (n-1) rounds
                int matchesPerRound = teamCount / 2; // number of matches per round

                var fixtures = new List<Models.Matches.Fixture>();
                DateTime currentDate = startDate;

                // --- FIRST LEG (standard round-robin rotation) ---
                for (int round = 0; round < rounds; round++)
                {
                    for (int match = 0; match < matchesPerRound; match++)
                    {
                        var home = teams[match];
                        var away = teams[teamCount - 1 - match];

                        // Skip if this is a BYE match
                        if (home.Id == -1 || away.Id == -1)
                            continue;

                        // Small tweak: sometimes swap home/away to balance
                        bool swap = (round % 2 == 1 && match == 0);

                        fixtures.Add(new Models.Matches.Fixture
                        {
                            GameSaveId = gameSaveId,
                            LeagueId = league.Id,
                            SeasonId = seasonId,
                            HomeTeamId = swap ? away.Id : home.Id,
                            AwayTeamId = swap ? home.Id : away.Id,
                            Date = currentDate,
                            Round = round + 1
                        });
                    }

                    // Rotate teams using "circle method"
                    var last = teams[teamCount - 1];
                    teams.RemoveAt(teamCount - 1);
                    teams.Insert(1, last);

                    // Increment week (7 days later)
                    currentDate = currentDate.AddDays(7);
                }

                // --- SECOND LEG (reverse fixtures) ---
                int firstLegRounds = rounds;
                foreach (var f in fixtures.ToList()) // copy list before adding to it
                {
                    fixtures.Add(new Models.Matches.Fixture
                    {
                        GameSaveId = f.GameSaveId,
                        LeagueId = f.LeagueId,
                        SeasonId = f.SeasonId,
                        HomeTeamId = f.AwayTeamId,
                        AwayTeamId = f.HomeTeamId,
                        Date = currentDate,
                        Round = f.Round + firstLegRounds
                    });

                    if (fixtures.Count % matchesPerRound == 0)
                        currentDate = currentDate.AddDays(7);
                }

                await _context.Fixtures.AddRangeAsync(fixtures);
            }

            await _context.SaveChangesAsync();
        }


    }
}