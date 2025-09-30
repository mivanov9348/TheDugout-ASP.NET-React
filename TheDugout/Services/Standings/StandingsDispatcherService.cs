

using TheDugout.Models.Fixtures;
using TheDugout.Services.League;

namespace TheDugout.Services.Standings
{
    public class StandingsDispatcherService : IStandingsDispatcherService
    {
        private readonly ILogger<StandingsDispatcherService> _logger;
        private readonly ILeagueStandingsService _leagueStandingsService;
        private readonly IEuropeanCupStandingService _eurocupStandingsService;

        public StandingsDispatcherService(ILogger<StandingsDispatcherService> logger, ILeagueStandingsService leagueStandingsService, IEuropeanCupStandingService eurocupStandingsService)
        {
            _logger = logger;
            _leagueStandingsService = leagueStandingsService;
            _eurocupStandingsService = eurocupStandingsService;
        }

        public async Task UpdateAfterMatchAsync(Models.Fixtures.Fixture fixture, CancellationToken ct = default)
        {
            switch (fixture.CompetitionType)
            {
                case CompetitionType.League:
                    if (fixture.LeagueId.HasValue)
                        await _leagueStandingsService.UpdateStandingsAfterMatchAsync(fixture);
                    break;

                //case CompetitionType.DomesticCup:
                //    if (fixture.CupRoundId.HasValue && fixture.CupRound?.PhaseTemplate?.IsGroupStage == true)
                //        await CupStandingsService.UpdateStandingsAfterMatchAsync(fixture.Id, ct);
                //    break;

                case CompetitionType.EuropeanCup:
                    if (fixture.EuropeanCupPhaseId.HasValue && fixture.EuropeanCupPhase?.PhaseTemplate?.IsKnockout == false)
                        await _eurocupStandingsService.UpdateEuropeanCupStandingsAfterMatchAsync(fixture.Id, ct);
                    break;

                default:
                    _logger.LogInformation("Fixture {FixtureId} has no standings update (knockout or unsupported).", fixture.Id);
                    break;
            }
        }
    }
}
