using TheDugout.Models.Matches;

namespace TheDugout.Services.MatchEngine
{
    public class MatchEngine : IMatchEngine
    {
        // Стартира нов мач
        public void StartMatch(Models.Matches.Match match)
        {
            // TODO: set minute=0, score=0:0, possession=random
        }

        // Симулира следваща минута
        public void NextMinute(Models.Matches.Match match)
        {
            // TODO:
            // - DeterminePossession()
            // - SimulateEvent()
            // - UpdateMatchState()
            // - CheckForEnd()
        }

        // Връща текущото състояние
        public int GetCurrentTurn(Models.Matches.Match match)
        {
            // TODO: return snapshot (minute, score, events, possession)
            throw new NotImplementedException();
        }

        // Приключва мача
        public void EndMatch(Models.Matches.Match match)
        {
            // TODO: set status=Played, finalize stats
        }

        // 🔹 Private helpers

        private Models.Teams.Team DeterminePossession(Models.Matches.Match match)
        {
            // TODO: random/probability logic
            throw new NotImplementedException();
        }

        private MatchEvent SimulateEvent(Models.Matches.Match match, Models.Teams.Team teamInPossession)
        {
            // TODO: attack/pass/goal/foul/etc
            throw new NotImplementedException();
        }

        private void UpdateMatchState(Models.Matches.Match match, MatchEvent ev)
        {
            // TODO: update score, stats, minute
        }

        private bool CheckForEnd(Models.Matches.Match match)
        {
            // TODO: return true if minute >= 90 (+ extra)
            throw new NotImplementedException();
        }
    }
}
