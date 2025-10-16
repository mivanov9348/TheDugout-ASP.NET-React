namespace TheDugout.Services.Match
{
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using TheDugout;
    using TheDugout.Data;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Models.Matches;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Match.Interfaces;
    public class MatchService : IMatchService
    {
        private readonly DugoutDbContext _context;

        public MatchService(DugoutDbContext context)
        {
            _context = context;
        }

        // -------------------------------
        // CREATE MATCH
        // -------------------------------
        public async Task<Match> CreateMatchFromFixtureAsync(Fixture fixture, GameSave gameSave)
        {
            if (fixture == null) throw new ArgumentNullException(nameof(fixture));
            if (gameSave == null) throw new ArgumentNullException(nameof(gameSave));

            var existingMatch = await _context.Matches
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                        .ThenInclude(t => t.Players)
                            .ThenInclude(p => p.Position)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                        .ThenInclude(t => t.Players)
                            .ThenInclude(p => p.Position)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.FixtureId == fixture.Id && m.GameSaveId == gameSave.Id)
                .ConfigureAwait(false);

            if (existingMatch != null)
                return existingMatch;

            var competition = await GetCompetitionForFixtureAsync(fixture).ConfigureAwait(false);
            if (competition == null)
                throw new InvalidOperationException("Competition not found for this fixture.");

            var match = new Match
            {
                GameSaveId = gameSave.Id,
                FixtureId = fixture.Id,
                Fixture = fixture,
                CurrentMinute = 0,
                Status = MatchStatus.Live,
                CompetitionId = competition.Id
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            await _context.Entry(match)
                .Reference(m => m.Fixture)
                .Query()
                .Include(f => f.HomeTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
                .Include(f => f.AwayTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
                .LoadAsync();

            return match;
        }

        public async Task<object?> GetMatchViewAsync(int fixtureId)
        {
            var fixture = await _context.Fixtures
                .AsNoTracking()
                .Include(f => f.HomeTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
                .Include(f => f.AwayTeam).ThenInclude(t => t.Players).ThenInclude(p => p.Position)
                .FirstOrDefaultAsync(f => f.Id == fixtureId)
                .ConfigureAwait(false);

            if (fixture == null)
                return null;

            var match = await _context.Matches
                .AsNoTracking()
                .Where(m => m.FixtureId == fixtureId)
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (match == null)
                return null;

            return BuildMatchView(match, fixture);
        }

        // -------------------------------
        // GET MATCH VIEW BY MATCH ID
        // -------------------------------
        public async Task<object?> GetMatchViewByIdAsync(int matchId)
        {
            var match = await _context.Matches
                .Include(m => m.Fixture)
                .ThenInclude(f => f.HomeTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
                .Include(m => m.Fixture.AwayTeam)
                    .ThenInclude(t => t.Players)
                        .ThenInclude(p => p.Position)
                .Include(m => m.Fixture.HomeTeam.TeamTactic)
                .Include(m => m.Fixture.AwayTeam.TeamTactic)
                .FirstOrDefaultAsync(m => m.Id == matchId)
                .ConfigureAwait(false);

            if (match == null || match.Fixture == null)
                return null;

            return BuildMatchView(match, match.Fixture);
        }


        // -------------------------------
        // COMPLETE MATCH
        // -------------------------------
        public async Task CompleteMatchAndSaveResultAsync(Match match, int homeGoals, int awayGoals)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var fixture = await _context.Fixtures
                .FirstOrDefaultAsync(f => f.Id == match.FixtureId)
                .ConfigureAwait(false);

            if (fixture == null)
                throw new InvalidOperationException("Fixture not found for this Match.");

            fixture.HomeTeamGoals = homeGoals;
            fixture.AwayTeamGoals = awayGoals;
            fixture.WinnerTeamId = homeGoals > awayGoals ? fixture.HomeTeamId :
                                  awayGoals > homeGoals ? fixture.AwayTeamId : null;
            fixture.Status = FixtureStatusEnum.Played;

            match.Status = MatchStatus.Played;
            match.CurrentMinute = 90;

            _context.Entry(fixture).State = EntityState.Modified;
            _context.Entry(match).State = EntityState.Modified;

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<Match> GetOrCreateMatchAsync(Fixture fixture, GameSave gameSave)
        {
            var existing = await _context.Matches
         .Include(m => m.Fixture)
             .ThenInclude(f => f.HomeTeam)
                 .ThenInclude(t => t.Players)
                     .ThenInclude(p => p.Position)
         .Include(m => m.Fixture)
             .ThenInclude(f => f.AwayTeam)
                 .ThenInclude(t => t.Players)
                     .ThenInclude(p => p.Position)
         .FirstOrDefaultAsync(m => m.FixtureId == fixture.Id && m.GameSaveId == gameSave.Id);

            if (existing != null)
                return existing;

            return await CreateMatchFromFixtureAsync(fixture, gameSave);
        }

        // -------------------------------
        // PRIVATE HELPERS
        // -------------------------------
        private async Task<Competition?> GetCompetitionForFixtureAsync(Fixture fixture)
        {
            return fixture.CompetitionType switch
            {
                CompetitionTypeEnum.League when fixture.LeagueId.HasValue =>
                    await _context.Competitions
                        .AsNoTracking()
                        .Include(c => c.League)
                        .FirstOrDefaultAsync(c =>
                            c.League != null &&
                            c.League.Id == fixture.LeagueId.Value &&
                            c.SeasonId == fixture.SeasonId)
                        .ConfigureAwait(false),

                CompetitionTypeEnum.DomesticCup when fixture.CupRoundId.HasValue =>
                    await _context.Competitions
                        .AsNoTracking()
                        .Include(c => c.Cup)
                        .ThenInclude(cup => cup.Rounds)
                        .FirstOrDefaultAsync(c =>
                            c.Cup != null &&
                            c.Cup.Rounds.Any(r => r.Id == fixture.CupRoundId.Value) &&
                            c.SeasonId == fixture.SeasonId)
                        .ConfigureAwait(false),

                CompetitionTypeEnum.EuropeanCup when fixture.EuropeanCupPhaseId.HasValue =>
                    await _context.Competitions
                        .AsNoTracking()
                        .Include(c => c.EuropeanCup)
                        .ThenInclude(ec => ec.Phases)
                        .FirstOrDefaultAsync(c =>
                            c.EuropeanCup != null &&
                            c.EuropeanCup.Phases.Any(p => p.Id == fixture.EuropeanCupPhaseId.Value) &&
                            c.SeasonId == fixture.SeasonId)
                        .ConfigureAwait(false),

                _ => throw new InvalidOperationException($"Unsupported or invalid competition type: {fixture.CompetitionType}")
            };
        }

        private static object BuildMatchView(Match match, Fixture fixture)
        {
            static object BuildTeamView(Team team)
            {
                var lineupIds = new HashSet<int>();
                var subsList = new List<object>();
                var tactic = team.TeamTactic;

                if (tactic != null)
                {
                    // Parse lineup
                    if (!string.IsNullOrWhiteSpace(tactic.LineupJson))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(tactic.LineupJson);
                            foreach (var el in doc.RootElement.EnumerateObject())
                            {
                                if (int.TryParse(el.Value.GetString(), out var id))
                                    lineupIds.Add(id);
                            }
                        }
                        catch { /* ignore invalid JSON */ }
                    }

                    // Parse substitutes
                    if (!string.IsNullOrWhiteSpace(tactic.SubstitutesJson))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(tactic.SubstitutesJson);
                            foreach (var el in doc.RootElement.EnumerateObject().OrderBy(e => e.Name))
                            {
                                if (int.TryParse(el.Value.GetString(), out var playerId))
                                {
                                    var p = team.Players.FirstOrDefault(x => x.Id == playerId);
                                    if (p != null)
                                    {
                                        subsList.Add(new
                                        {
                                            slot = el.Name,
                                            id = p.Id,
                                            number = p.KitNumber,
                                            position = p.Position?.Code ?? "N/A",
                                            name = $"{p.FirstName} {p.LastName}",
                                            stats = new { goals = 0, passes = 0 }
                                        });
                                    }
                                }
                            }
                        }
                        catch { /* ignore invalid JSON */ }
                    }
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

                return new
                {
                    name = team.Name,
                    starters,
                    subs = subsList
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
}