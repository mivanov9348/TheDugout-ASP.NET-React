using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Linq;
using TheDugout.Data;
using TheDugout.Models.Competitions;
using TheDugout.Models.Game;
using TheDugout.Models.Matches;

namespace TheDugout.Services.Cup
{
    public class CupService : ICupService
    {
        private readonly DugoutDbContext _context;
        private readonly Random _random = new Random();

        public CupService(DugoutDbContext context)
        {
            _context = context;
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

                Console.WriteLine($"[INFO] Created cup {template.Name} with {teamsCount} teams");

                await GenerateCupFixturesAsync(cup, seasonId, gameSave.Id);
            }
        }

        public async Task GenerateCupFixturesAsync(Models.Competitions.Cup cup, int seasonId, int gameSaveId)
        {
            var teams = cup.Teams.Select(ct => new Models.Teams.Team
            {
                Id = ct.TeamId,
                Name = ct.Team?.Name ?? $"Team {ct.TeamId}"
            }).ToList();

            int teamsCount = teams.Count;

            if (teamsCount < 2)
            {
                Console.WriteLine($"[WARN] Cup {cup.Id}: Not enough teams ({teamsCount})");
                return;
            }

            int nextPowerOfTwo = (int)Math.Pow(2, Math.Ceiling(Math.Log2(teamsCount)));
            int prelimTeams = 2 * (teamsCount - (nextPowerOfTwo / 2));
            int prelimMatches = prelimTeams / 2;

            teams = teams.OrderBy(t => _random.Next()).ToList();

            var roundNumber = 1;
            var advancingTeams = new List<Models.Teams.Team>(teams);

            // --- Preliminary round ---
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
                    var home = advancingTeams[i * 2];
                    var away = advancingTeams[i * 2 + 1];

                    prelimRound.Fixtures.Add(new Models.Matches.Fixture
                    {
                        GameSaveId = gameSaveId,
                        SeasonId = seasonId,
                        CompetitionType = CompetitionType.DomesticCup,
                        CupRound = prelimRound,
                        HomeTeamId = home.Id,
                        AwayTeamId = away.Id,
                        Date = DateTime.UtcNow.AddDays(roundNumber * 7),
                        Round = roundNumber,
                        Status = MatchStatus.Scheduled
                    });
                }

                _context.CupRounds.Add(prelimRound);
                await _context.SaveChangesAsync();

                return; 
            }

            string roundName = teamsCount switch
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

            advancingTeams = advancingTeams.OrderBy(t => _random.Next()).ToList();

            for (int i = 0; i < advancingTeams.Count; i += 2)
            {
                var home = advancingTeams[i];
                var away = advancingTeams[i + 1];

                round.Fixtures.Add(new Models.Matches.Fixture
                {
                    GameSaveId = gameSaveId,
                    SeasonId = seasonId,
                    CompetitionType = CompetitionType.DomesticCup,
                    CupRound = round,
                    HomeTeamId = home.Id,
                    AwayTeamId = away.Id,
                    Date = DateTime.UtcNow.AddDays(roundNumber * 7),
                    Round = roundNumber,
                    Status = MatchStatus.Scheduled
                });
            }

            _context.CupRounds.Add(round);
            await _context.SaveChangesAsync();
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

            // намираме последния рунд
            var lastRound = cup.Rounds
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefault();

            if (lastRound == null)
            {
                Console.WriteLine($"[WARN] Cup {cupId} has no rounds yet.");
                return;
            }

            // проверка дали всички мачове са приключили и имат WinnerTeamId
            if (lastRound.Fixtures.Any(f => f.Status != MatchStatus.Played || f.WinnerTeamId == null))
            {
                Console.WriteLine($"[WARN] Not all fixtures in {lastRound.Name} are finished or have winners.");
                return;
            }

            // взимаме победителите директно
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

            // нов рунд
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

            // разбъркваме победителите
            winners = winners.OrderBy(t => _random.Next()).ToList();

            // ако са нечетен брой – bye
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

                newRound.Fixtures.Add(new Models.Matches.Fixture
                {
                    GameSaveId = gameSaveId,
                    SeasonId = seasonId,
                    CompetitionType = CompetitionType.DomesticCup,
                    CupRound = newRound,
                    HomeTeamId = home.Id,
                    AwayTeamId = away.Id,
                    Date = DateTime.UtcNow.AddDays(nextRoundNumber * 7),
                    Round = nextRoundNumber,
                    Status = MatchStatus.Scheduled
                });
            }

            _context.CupRounds.Add(newRound);
            await _context.SaveChangesAsync();

            if (byeTeam != null)
                Console.WriteLine($"[INFO] {byeTeam.Name} gets a bye to {roundName}");
        }

    }
}
