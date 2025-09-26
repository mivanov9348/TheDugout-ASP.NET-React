using Microsoft.EntityFrameworkCore;
using TheDugout;
using TheDugout.Data;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Models.Teams;
using TheDugout.Services.Match;

public class MatchService : IMatchService
{
    private readonly DugoutDbContext _context;

    public MatchService(DugoutDbContext context)
    {
        _context = context;
    }

    public async Task<TheDugout.Models.Matches.Match> CreateMatchFromFixtureAsync(Fixture fixture, GameSave gameSave)
    {
        if (fixture == null) throw new ArgumentNullException(nameof(fixture));
        if (gameSave == null) throw new ArgumentNullException(nameof(gameSave));

        var match = new Match
        {
            GameSaveId = gameSave.Id,
            GameSave = gameSave,
            FixtureId = fixture.Id,
            Fixture = fixture,
            CurrentMinute = 0,
            Status = MatchStatus.Live
        };

        fixture.Matches.Add(match);

        _context.Add(match);
        await _context.SaveChangesAsync();

        return match;
    }

    public async Task<object?> GetMatchViewAsync(int fixtureId)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.HomeTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
            .Include(f => f.AwayTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
            .Include(f => f.Matches)
            .FirstOrDefaultAsync(f => f.Id == fixtureId);

        if (fixture == null) return null;

        var match = fixture.Matches.OrderByDescending(m => m.Id).FirstOrDefault();
        if (match == null) return null;

        return BuildMatchView(match, fixture);
    }

    public async Task<object?> GetMatchViewByIdAsync(int matchId)
    {
        var match = await _context.Matches
                   .Include(m => m.Fixture).ThenInclude(f => f.HomeTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
                   .Include(m => m.Fixture).ThenInclude(f => f.HomeTeam).ThenInclude(t => t.TeamTactic)
                   .Include(m => m.Fixture).ThenInclude(f => f.AwayTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
                   .Include(m => m.Fixture).ThenInclude(f => f.AwayTeam).ThenInclude(t => t.TeamTactic)
                   .FirstOrDefaultAsync(m => m.Id == matchId);


        if (match == null) return null;

        return BuildMatchView(match, match.Fixture);
    }

    public async Task CompleteMatchAndSaveResultAsync(Match match, int homeGoals, int awayGoals)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        var fixture = await _context.Set<Fixture>()
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .FirstOrDefaultAsync(f => f.Id == match.FixtureId);

        if (fixture == null)
            throw new InvalidOperationException("Fixture не е намерен за този Match.");

        fixture.HomeTeamGoals = homeGoals;
        fixture.AwayTeamGoals = awayGoals;

        fixture.WinnerTeamId = homeGoals > awayGoals ? fixture.HomeTeamId :
                              awayGoals > homeGoals ? fixture.AwayTeamId : null;

        fixture.Status = FixtureStatus.Played;
        match.Status = MatchStatus.Played;
        match.CurrentMinute = 90;

        _context.Update(match);
        _context.Update(fixture);

        await _context.SaveChangesAsync();
    }

    private static object BuildMatchView(Match match, Fixture fixture)
    {
        object BuildTeamView(Team team)
        {
            var tactic = team.TeamTactic; 
            var lineupIds = new HashSet<int>();

            if (tactic != null && !string.IsNullOrWhiteSpace(tactic.LineupJson))
            {
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(tactic.LineupJson);
                    if (dict != null)
                    {
                        foreach (var val in dict.Values)
                        {
                            if (int.TryParse(val, out var id))
                                lineupIds.Add(id);
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }

            var starters = team.Players
                .Where(p => lineupIds.Contains(p.Id))
                .Select(p => new
                {
                    id = p.Id,
                    number = p.KitNumber,
                    position = p.Position?.Code ?? "N/A",
                    name = $"{p.FirstName} {p.LastName}",
                    stats = new { goals = 0, passes = 0 }
                });

            var substitutes = team.Players
                .Where(p => !lineupIds.Contains(p.Id))
                .Select(p => new
                {
                    id = p.Id,
                    number = p.KitNumber,
                    position = p.Position?.Code ?? "N/A",
                    name = $"{p.FirstName} {p.LastName}",
                    stats = new { goals = 0, passes = 0 }
                });

            return new
            {
                name = team.Name,
                starters,
                substitutes
            };
        }

        return new
        {
            MatchId = match.Id,
            FixtureId = fixture.Id,
            HomeTeam = BuildTeamView(fixture.HomeTeam),
            AwayTeam = BuildTeamView(fixture.AwayTeam),
            Score = new { home = fixture.HomeTeamGoals ?? 0, away = fixture.AwayTeamGoals ?? 0 },
            Status = match.Status.ToString(),
            Minute = match.CurrentMinute
        };
    }

}
