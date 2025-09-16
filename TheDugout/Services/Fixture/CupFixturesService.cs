
using Microsoft.EntityFrameworkCore;
using System;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Matches;

namespace TheDugout.Services.Fixture
{
    public class CupFixturesService : ICupFixturesService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixtureHelperService;
        private readonly Random _random = new Random();
        public CupFixturesService(DugoutDbContext context, IFixturesHelperService fixtureHelperService)
        {
            _context = context;
            _fixtureHelperService = fixtureHelperService;
        }

        public async Task GenerateCupFixturesAsync(Models.Competitions.Cup cup, int seasonId, int gameSaveId)
        {
            var teams = cup.Teams.Select(ct => new Models.Teams.Team
            {
                Id = ct.TeamId,
                Name = ct.Team?.Name ?? $"Team {ct.TeamId}"
            }).ToList();

            if (teams.Count < 2) return;

            int nextPowerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log2(teams.Count)));
            int prelimTeams = 2 * (teams.Count - (nextPowerOfTwo / 2));
            int prelimMatches = prelimTeams / 2;

            teams = teams.OrderBy(t => _random.Next()).ToList();
            int roundNumber = 1;

            if (prelimMatches > 0)
            {
                var prelimRound = new CupRound
                {
                    CupId = cup.Id,
                    RoundNumber = roundNumber,
                    Name = "Preliminary Round"
                };

                for (int i = 0; i < prelimMatches; i++)
                {
                    var home = teams[i * 2];
                    var away = teams[i * 2 + 1];

                    prelimRound.Fixtures.Add(_fixtureHelperService.CreateFixture(
                        gameSaveId,
                        seasonId,
                        home.Id,
                        away.Id,
                        DateTime.UtcNow.AddDays(roundNumber * 7),
                        roundNumber,
                        CompetitionType.DomesticCup,
                        prelimRound
                    ));
                }

                _context.CupRounds.Add(prelimRound);
                await _context.SaveChangesAsync();
                return;
            }

            string roundName = teams.Count switch
            {
                2 => "Final",
                4 => "Semifinals",
                8 => "Quarterfinals",
                16 => "Round of 16",
                32 => "Round of 32",
                64 => "Round of 64",
                _ => $"Round {roundNumber}"
            };

            var round = new CupRound
            {
                CupId = cup.Id,
                RoundNumber = roundNumber,
                Name = roundName
            };

            teams = teams.OrderBy(t => _random.Next()).ToList();

            for (int i = 0; i < teams.Count; i += 2)
            {
                var home = teams[i];
                var away = teams[i + 1];

                round.Fixtures.Add(_fixtureHelperService.CreateFixture(
                    gameSaveId,
                    seasonId,
                    home.Id,
                    away.Id,
                    DateTime.UtcNow.AddDays(roundNumber * 7),
                    roundNumber,
                    CompetitionType.DomesticCup,
                    round
                ));
            }

            _context.CupRounds.Add(round);
            await _context.SaveChangesAsync();
        }
    }
}
