namespace TheDugout.Services.Fixture
{
    using Microsoft.EntityFrameworkCore;    
    using TheDugout.Data;
    using TheDugout.Models.Cups;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;

    public class FixturesHelperService : IFixturesHelperService
    {
        private readonly DugoutDbContext _context;
        private static readonly Random _random = new Random();

        public FixturesHelperService(DugoutDbContext context)
        {
            _context = context;
        }

        public Fixture CreateFixture(
            int gameSaveId,
            int? seasonId,
            int? homeTeamId,
            int? awayTeamId,
            DateTime date,
            int round,
            CompetitionTypeEnum competitionType,
            CupRound? cupRound = null,
            int? leagueId = null,
            int? europeanCupPhaseId = null)
        {
            return new Models.Fixtures.Fixture
            {
                GameSaveId = gameSaveId,
                SeasonId = seasonId,
                CompetitionType = competitionType,
                CupRound = cupRound,
                LeagueId = leagueId,
                EuropeanCupPhaseId = europeanCupPhaseId,
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                HomeTeamGoals = 0,
                AwayTeamGoals = 0,
                IsElimination = DetermineIsElimination(competitionType, cupRound, europeanCupPhaseId),
                Date = date,
                Round = round,
                Status = FixtureStatusEnum.Scheduled
            };
        }

        public List<(int, int)>? TryFindRoundPairing(List<int> teamIds, HashSet<string> existingPairs, int maxAttempts)
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

            // не успяхме да намерим без повторение
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

                if (best == -1)
                    best = remaining.First();

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

        public string GetRoundName(int teamsCount, int roundNumber, int totalRounds, bool hasPrelim = false)
        {
            if (hasPrelim && roundNumber == 1)
                return "Preliminary Round";

            int adjustedRound = hasPrelim ? roundNumber - 1 : roundNumber;

            if (adjustedRound == totalRounds)
                return "Final";

            if (adjustedRound == totalRounds - 1)
                return "Semi Final";

            if (adjustedRound == totalRounds - 2)
                return "Quarter Final";

            // teams left in this round: 2^(totalRounds - adjustedRound + 1)
            int teamsInThisRound = (int)Math.Pow(2, totalRounds - adjustedRound + 1);
            return $"Round of {teamsInThisRound}";
        }
        private bool DetermineIsElimination(CompetitionTypeEnum competitionType, CupRound? cupRound, int? europeanCupPhaseId)
        {
            switch (competitionType)
            {
                case CompetitionTypeEnum.League:
                    return false;

                case CompetitionTypeEnum.DomesticCup:
                    return true;

                case CompetitionTypeEnum.EuropeanCup:
                    if (europeanCupPhaseId.HasValue)
                    {
                        var phase = _context.EuropeanCupPhases
                            .Include(p => p.PhaseTemplate)
                            .FirstOrDefault(p => p.Id == europeanCupPhaseId.Value);

                        return phase?.PhaseTemplate.IsKnockout ?? false;
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
