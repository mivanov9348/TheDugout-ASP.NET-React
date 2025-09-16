using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Linq;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;
using TheDugout.Services.Fixture;

namespace TheDugout.Services.Cup
{
    public class CupService : ICupService
    {
        private readonly DugoutDbContext _context;
        private readonly IFixturesHelperService _fixturesHelperService;
        private readonly ICupFixturesService _cupFixturesService;
        private readonly Random _random = new Random();

        public CupService(DugoutDbContext context, ICupFixturesService cupFixturesService, IFixturesHelperService fixturesHelperService)
        {
            _context = context;
            _cupFixturesService = cupFixturesService;
            _fixturesHelperService = fixturesHelperService;
        }

        public async Task InitializeCupsForGameSaveAsync(GameSave gameSave, int seasonId)
        {
            var cupTemplates = await _context.CupTemplates
                .Where(ct => ct.IsActive)
                .ToListAsync();

            foreach (var template in cupTemplates)
            {
                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code == template.CountryCode);
                if (country == null)
                {
                    Console.WriteLine($"[WARN] Country not found for template {template.Name} ({template.CountryCode})");
                    continue;
                }

                var teams = gameSave.Teams
                    .Where(t => t.CountryId == country.Id)
                    .ToList();

                if (teams.Count < 2)
                {
                    Console.WriteLine($"[WARN] Not enough teams for cup {template.Name}. Found: {teams.Count}");
                    continue;
                }

                int teamsCount = teams.Count;
                int nextPowerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log2(teamsCount)));
                int roundsCount = (int)Math.Ceiling(Math.Log2(nextPowerOfTwo));

                var cup = new Models.Competitions.Cup
                {
                    TemplateId = template.Id,
                    GameSaveId = gameSave.Id,
                    SeasonId = seasonId,
                    CountryId = country.Id,
                    TeamsCount = teamsCount,
                    RoundsCount = roundsCount,
                    IsActive = true
                };

                foreach (var team in teams)
                    cup.Teams.Add(new CupTeam { TeamId = team.Id });

                _context.Cups.Add(cup);
                await _context.SaveChangesAsync();


                await _cupFixturesService.GenerateCupFixturesAsync(cup, seasonId, gameSave.Id);
            }
        }

        public async Task GenerateNextRoundAsync(int cupId, int seasonId, int gameSaveId)
        {
            var cup = await _context.Cups
                .Include(c => c.Rounds)
                    .ThenInclude(r => r.Fixtures)
                .FirstOrDefaultAsync(c => c.Id == cupId);

            if (cup == null)
            {
                Console.WriteLine($"[WARN] Cup {cupId} not found.");
                return;
            }

            var lastRound = cup.Rounds
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefault();

            if (lastRound == null)
            {
                Console.WriteLine($"[WARN] Cup {cupId} has no rounds yet.");
                return;
            }

            if (lastRound.Fixtures.Any(f => f.Status != MatchStatus.Played || f.WinnerTeamId == null))
            {
                Console.WriteLine($"[WARN] Not all fixtures in {lastRound.Name} are finished or have winners.");
                return;
            }

            var winnerIds = lastRound.Fixtures
                .Select(f => f.WinnerTeamId!.Value)
                .ToList();

            var winners = await _context.Teams
                .Where(t => winnerIds.Contains(t.Id))
                .ToListAsync();

            if (winners.Count < 2)
            {
                Console.WriteLine($"[INFO] Cup {cupId} finished, champion is {winners.FirstOrDefault()?.Name}");
                return;
            }

            int nextRoundNumber = lastRound.RoundNumber + 1;
            string roundName = winners.Count switch
            {
                2 => "Final",
                4 => "Semifinals",
                8 => "Quarterfinals",
                16 => "Round of 16",
                32 => "Round of 32",
                64 => "Round of 64",
                _ => $"Round {nextRoundNumber}"
            };

            var newRound = new CupRound
            {
                CupId = cup.Id,
                RoundNumber = nextRoundNumber,
                Name = roundName
            };

            winners = winners.OrderBy(t => _random.Next()).ToList();

            Models.Teams.Team? byeTeam = null;
            if (winners.Count % 2 != 0)
            {
                byeTeam = winners.Last();
                winners.Remove(byeTeam);
            }

            for (int i = 0; i < winners.Count; i += 2)
            {
                var home = winners[i];
                var away = winners[i + 1];

                var fixture = _fixturesHelperService.CreateFixture(
                    gameSaveId,
                    seasonId,
                    home.Id,
                    away.Id,
                    DateTime.UtcNow.AddDays(nextRoundNumber * 7),
                    nextRoundNumber,
                    CompetitionType.DomesticCup,
                    newRound
                );

                newRound.Fixtures.Add(fixture);
            }

            _context.CupRounds.Add(newRound);
            await _context.SaveChangesAsync();

            if (byeTeam != null)
                Console.WriteLine($"[INFO] {byeTeam.Name} gets a bye to {roundName}");
        }


    }
}
