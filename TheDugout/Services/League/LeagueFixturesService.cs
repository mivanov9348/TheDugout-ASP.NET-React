namespace TheDugout.Services.League
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Fixture.Interfaces;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Season.Interfaces;
    public class LeagueFixturesService : ILeagueFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly ILeagueScheduleService _leagueScheduleService;
        public LeagueFixturesService(DugoutDbContext context, IFixturesHelperService fixturesHelperService, ILeagueScheduleService leagueScheduleService)
        {
            _context = context;
            _fixturesHelperService = fixturesHelperService;
            _leagueScheduleService = leagueScheduleService;
        }
        public async Task GenerateLeagueFixturesAsync(int gameSaveId, int seasonId, DateTime startDate)
        {
            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == seasonId);

            var leagues = await _context.Leagues
                .Include(l => l.Teams)
                .Where(l => l.GameSaveId == gameSaveId && l.SeasonId == seasonId)                
                .ToListAsync();

            foreach (var league in leagues)
            {
                var fixtures = GenerateLeagueFixturesCore(
                    gameSaveId,
                    seasonId,
                    league.Id,
                    league.Teams.ToList(),
                    _fixturesHelperService);

                _leagueScheduleService.AssignLeagueFixtures(fixtures, season);

                await _context.Fixtures.AddRangeAsync(fixtures);
            }

            await _context.SaveChangesAsync();
        }
        private List<Fixture> GenerateLeagueFixturesCore(
    int gameSaveId,
    int seasonId,
    int leagueId,
    List<Team> teams,
    IFixturesHelperService fixturesHelperService)
        {
            if (teams.Count % 2 != 0)
                teams.Add(new Models.Teams.Team { Id = -1, Name = "BYE" });

            int teamCount = teams.Count;
            int rounds = teamCount - 1;
            int matchesPerRound = teamCount / 2;

            var fixtures = new List<Models.Fixtures.Fixture>();

            // --- First leg ---
            var rotation = new List<Models.Teams.Team>(teams);
            for (int round = 0; round < rounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    var home = rotation[match];
                    var away = rotation[teamCount - 1 - match];
                    if (home.Id == -1 || away.Id == -1) continue;

                    bool swap = round % 2 == 1 && match == 0;

                    fixtures.Add(fixturesHelperService.CreateFixture(
                        gameSaveId,
                        seasonId,
                        swap ? away.Id : home.Id,
                        swap ? home.Id : away.Id,
                        DateTime.MinValue, 
                        round + 1,
                        CompetitionTypeEnum.League,
                        leagueId: leagueId
                    ));
                }

                // Rotate
                var last = rotation[teamCount - 1];
                rotation.RemoveAt(teamCount - 1);
                rotation.Insert(1, last);
            }

            // --- Second leg ---
            int firstLegRounds = rounds;
            foreach (var f in fixtures.ToList())
            {
                fixtures.Add(fixturesHelperService.CreateFixture(
                    f.GameSaveId,
                    f.SeasonId,
                    f.AwayTeamId,
                    f.HomeTeamId,
                    DateTime.MinValue, 
                    f.Round + firstLegRounds,
                    CompetitionTypeEnum.League,
                    leagueId: f.LeagueId
                ));
            }

            return fixtures;
        }
    }
}
