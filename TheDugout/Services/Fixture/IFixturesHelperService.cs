using TheDugout.Models.Competitions;
using TheDugout.Models.Matches;

namespace TheDugout.Services.Fixture
{
    public interface IFixturesHelperService
    {
        Models.Matches.Fixture CreateFixture(
            int gameSaveId,
            int seasonId,
            int homeTeamId,
            int awayTeamId,
            DateTime date,
            int round,
            CompetitionType competitionType,
            CupRound? cupRound = null,
            int? leagueId = null,
            int? europeanCupPhaseId = null);

        List<(int, int)> TryFindRoundPairing(List<int> teamIds, HashSet<string> existingPairs, int maxAttempts);

        List<(int, int)> GreedyPairingMinimizeRepeats(List<int> teamIds, HashSet<string> existingPairs);

        string PairKey(int a, int b);

        int DecideHome(int a, int b, Dictionary<int, int> homeCount);

    }
}
