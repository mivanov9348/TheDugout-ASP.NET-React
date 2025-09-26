using System;
using TheDugout.Models.Competitions;
using TheDugout.Models.Fixtures;

namespace TheDugout.Services.Fixture
{
    public class FixturesHelperService : IFixturesHelperService
    {
        private static readonly Random _random = new Random();
        public FixturesHelperService()
        {
        }

        public Models.Fixtures.Fixture CreateFixture(
          int gameSaveId,
          int seasonId,
          int homeTeamId,
          int awayTeamId,
          DateTime date,
          int round,
          CompetitionType competitionType,
          CupRound? cupRound = null,
          int? leagueId = null,
          int? europeanCupPhaseId = null
      )
        {
            return new Models.Matches.Fixture
            {
                GameSaveId = gameSaveId,
                SeasonId = seasonId,
                CompetitionType = competitionType,
                CupRound = cupRound,
                LeagueId = leagueId,
                EuropeanCupPhaseId = europeanCupPhaseId,
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                Date = date,
                Round = round,
                Status = FixtureStatus.Scheduled
            };
        }

        public List<(int, int)> TryFindRoundPairing(List<int> teamIds, HashSet<string> existingPairs, int maxAttempts)
        {
            var rnd = _random;
            var n = teamIds.Count;
            if (n % 2 != 0) throw new InvalidOperationException("Team count must be even for pairing.");

            var working = teamIds.ToArray();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // shuffle Fisher-Yates
                for (int i = n - 1; i > 0; i--)
                {
                    int j = rnd.Next(i + 1);
                    (working[i], working[j]) = (working[j], working[i]);
                }

                bool ok = true;
                var pairs = new List<(int, int)>(n / 2);
                for (int i = 0; i < n; i += 2)
                {
                    var a = working[i];
                    var b = working[i + 1];
                    if (existingPairs.Contains(PairKey(a, b)))
                    {
                        ok = false;
                        break;
                    }
                    pairs.Add((a, b));
                }
                if (ok) return pairs;
            }

            return null;
        }

        public List<(int, int)> GreedyPairingMinimizeRepeats(List<int> teamIds, HashSet<string> existingPairs)
        {
            var remaining = new HashSet<int>(teamIds);
            var pairs = new List<(int, int)>();
            while (remaining.Count > 0)
            {
                int a = remaining.First();
                remaining.Remove(a);

                int best = -1;
                foreach (var cand in remaining)
                {
                    if (!existingPairs.Contains(PairKey(a, cand)))
                    {
                        best = cand;
                        break;
                    }
                }

                if (best == -1) best = remaining.First();

                remaining.Remove(best);
                pairs.Add((a, best));
            }
            return pairs;
        }

        public string PairKey(int a, int b) => a < b ? $"{a}:{b}" : $"{b}:{a}";

        public int DecideHome(int a, int b, Dictionary<int, int> homeCount)
        {
            var aHome = homeCount.TryGetValue(a, out var ac) ? ac : 0;
            var bHome = homeCount.TryGetValue(b, out var bc) ? bc : 0;
            if (aHome < bHome) return a;
            if (bHome < aHome) return b;
            return _random.Next(2) == 0 ? a : b;
        }
    }
}
