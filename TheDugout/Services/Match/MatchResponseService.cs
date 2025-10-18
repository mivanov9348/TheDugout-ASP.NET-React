namespace TheDugout.Services.Match
{
    using TheDugout.Models.Enums;
    using TheDugout.Models.Fixtures;
    using TheDugout.Models.Game;
    using TheDugout.Services.Match.Interfaces;

    public class MatchResponseService : IMatchResponseService
    {
        public async Task<object> GetFormattedMatchResponseAsync(Fixture fixture, GameSave gameSave)
        {
            var homePens = fixture.Match?.Penalties.Count(p => p.TeamId == fixture.HomeTeamId && p.IsScored) ?? 0;
            var awayPens = fixture.Match?.Penalties.Count(p => p.TeamId == fixture.AwayTeamId && p.IsScored) ?? 0;

            string? winner = null;
            if (fixture.HomeTeamGoals > fixture.AwayTeamGoals)
            {
                winner = fixture.HomeTeam?.Name;
            }
            else if (fixture.AwayTeamGoals > fixture.HomeTeamGoals)
            {
                winner = fixture.AwayTeam?.Name;
            }
            else if (fixture.IsElimination && (homePens > 0 || awayPens > 0))
            {
                winner = homePens > awayPens ? fixture.HomeTeam?.Name : fixture.AwayTeam?.Name;
            }

            return new
            {
                FixtureId = fixture.Id,
                CompetitionName = GetCompetitionName(fixture),
                Home = fixture.HomeTeam?.Name ?? "Unknown Team",
                Away = fixture.AwayTeam?.Name ?? "Unknown Team",
                HomeGoals = fixture.HomeTeamGoals,
                AwayGoals = fixture.AwayTeamGoals,
                Status = fixture.Match != null ? (int)fixture.Match.Status : (int)fixture.Status,
                IsUserTeamMatch = (fixture.HomeTeamId == gameSave.UserTeamId || fixture.AwayTeamId == gameSave.UserTeamId),
                HomePenalties = homePens,
                AwayPenalties = awayPens,
                Winner = winner,
                IsElimination = fixture.CompetitionType == CompetitionTypeEnum.DomesticCup
                             || fixture.CompetitionType == CompetitionTypeEnum.EuropeanCup
            };
        }

        public async Task<List<object>> GetFormattedMatchesResponseAsync(List<Fixture> fixtures, GameSave gameSave)
        {
            var matchResponses = new List<object>();
            foreach (var fixture in fixtures)
            {
                var response = await GetFormattedMatchResponseAsync(fixture, gameSave);
                matchResponses.Add(response);
            }
            return matchResponses;
        }

        public string GetCompetitionName(Fixture fixture)
        {
            try
            {
                return fixture.CompetitionType switch
                {
                    CompetitionTypeEnum.League => fixture.League?.Template?.Name ?? "Unknown League",
                    CompetitionTypeEnum.DomesticCup => fixture.CupRound?.Cup?.Template?.Name ?? "Unknown Cup",
                    CompetitionTypeEnum.EuropeanCup => fixture.EuropeanCupPhase?.EuropeanCup?.Template?.Name ?? "Unknown European Cup",
                    _ => "Unknown Competition"
                };
            }
            catch
            {
                return "Unknown Competition";
            }
        }
    }
}