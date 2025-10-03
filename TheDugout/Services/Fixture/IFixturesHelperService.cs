// IFixturesHelperService.cs
using System;
using System.Collections.Generic;
using TheDugout.Models.Cups;
using TheDugout.Models.Fixtures;

namespace TheDugout.Services.Fixture
{
    public interface IFixturesHelperService
    {
        Models.Fixtures.Fixture CreateFixture(
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

        List<(int, int)>? TryFindRoundPairing(List<int> teamIds, HashSet<string> existingPairs, int maxAttempts);
        List<(int, int)> GreedyPairingMinimizeRepeats(List<int> teamIds, HashSet<string> existingPairs);
        string PairKey(int a, int b);
        int DecideHome(int a, int b, Dictionary<int, int> homeCount);
        string GetRoundName(int teamsCount, int roundNumber, int totalRounds, bool hasPrelim = false);
    }
}
