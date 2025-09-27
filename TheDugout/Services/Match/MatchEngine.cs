using TheDugout.Data;
using TheDugout.Models.Fixtures;
using TheDugout.Models.Matches;
using TheDugout.Services.Match;
using TheDugout.Services.Team;

namespace TheDugout.Services.MatchEngine
{
    public class MatchEngine : IMatchEngine
    {
        private readonly Random _random = new Random();
        private readonly ITeamPlanService _teamPlanService;
        private readonly IMatchEventService _matchEventService;
        private readonly DugoutDbContext _context;
        public MatchEngine(ITeamPlanService teamPlanService, IMatchEventService matchEventService, DugoutDbContext context)
        {
            _teamPlanService = teamPlanService;
            _matchEventService = matchEventService;
            _context = context;
        }

        public void StartMatch(Models.Matches.Match match)
        {
            match.Status = MatchStatus.Live;
            match.CurrentMinute = 0;
            //match.Fixture.Status = FixtureStatus.Scheduled;
            match.Fixture.HomeTeamGoals = 0;
            match.Fixture.AwayTeamGoals = 0;

            match.CurrentTurn = MatchTurn.Home;
        }

        public void EndMatch(Models.Matches.Match match)
        {
            match.Status = MatchStatus.Played;
            match.Fixture.Status = FixtureStatus.Played;

            if (match.Fixture.HomeTeamGoals > match.Fixture.AwayTeamGoals)
                match.Fixture.WinnerTeamId = match.Fixture.HomeTeamId;
            else if (match.Fixture.AwayTeamGoals > match.Fixture.HomeTeamGoals)
                match.Fixture.WinnerTeamId = match.Fixture.AwayTeamId;
            else
                match.Fixture.WinnerTeamId = null;
        }

        public void PlayNextMinute(Models.Matches.Match match)
        {
            int increment = _random.Next(1, 11);
            match.CurrentMinute += increment;
        }

        public void ChangeTurn(Models.Matches.Match match)
        {
            match.CurrentTurn = match.CurrentTurn == MatchTurn.Home
                ? MatchTurn.Away
                : MatchTurn.Home;
        }

        public bool IsMatchFinished(Models.Matches.Match match)
        {
            return match.CurrentMinute >= 90;
        }

        public async Task RunMatch(Models.Matches.Match match)
        {
            StartMatch(match);

            while (!IsMatchFinished(match))
            {
                // 1. Get match minute
                PlayNextMinute(match);

                // 2. Determine which team is in possession
                var currentTeamId = match.CurrentTurn == MatchTurn.Home
                   ? match.Fixture.HomeTeamId
                   : match.Fixture.AwayTeamId;

                // 3. Get random player from the team in possession
                var currentTeam = _context.Teams
                    .FirstOrDefault(t => t.Id == currentTeamId);

                if (currentTeam == null)
                    throw new InvalidOperationException($"Team with Id {currentTeamId} not found.");

                var lineup = await _teamPlanService.GetStartingLineupAsync(currentTeam);

                if (lineup == null || !lineup.Any())
                    throw new InvalidOperationException($"No starting lineup found for team {currentTeam.Id}");

                var outfieldPlayers = lineup.Where(p => p.PositionCode != "GK").ToList();

                if (!outfieldPlayers.Any())
                    throw new InvalidOperationException($"No outfield players found for team {currentTeam.Id}");

                var player = outfieldPlayers[_random.Next(outfieldPlayers.Count)];

                // 4. Get random event
                var eventType = _matchEventService.GetRandomEvent();

                // 5. Calculate outcome based on team plan and player stats
                // TODO: implement outcome calculation

                // 6. Generate commentary
                // TODO: implement commentary

                // 7. Log event to match events
                // TODO: implement match event logging

                // 8. Update Stats
                // TODO: implement stats update

                // 9. Change turn
                ChangeTurn(match);

                // 10. Check if match is finished
                if (IsMatchFinished(match))
                {
                    EndMatch(match);
                    break;
                }
            }

            EndMatch(match);
        }

    }
}
