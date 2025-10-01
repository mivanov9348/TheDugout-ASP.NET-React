

using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Fixtures;
using TheDugout.Services.Cup;
using TheDugout.Services.EuropeanCup;
using TheDugout.Services.League;

namespace TheDugout.Services.Standings
{
    public class StandingsDispatcherService : IStandingsDispatcherService
    {
        private readonly ILogger<StandingsDispatcherService> _logger;
        private readonly DugoutDbContext _context;
        private readonly ILeagueStandingsService _leagueStandingsService;
        private readonly ICupFixturesService _cupFixturesService;
        private readonly IEuropeanCupStandingService _eurocupStandingsService;

        public StandingsDispatcherService(ILogger<StandingsDispatcherService> logger, DugoutDbContext context, ILeagueStandingsService leagueStandingsService, ICupFixturesService cupFixturesService, IEuropeanCupStandingService eurocupStandingsService)
        {
            _logger = logger;
            _context = context;
            _leagueStandingsService = leagueStandingsService;
            _cupFixturesService = cupFixturesService;
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

                case CompetitionType.DomesticCup:
                    if (fixture.CupRoundId.HasValue)
                    {
                        var cupRound = await _context.CupRounds
                            .Include(cr => cr.Fixtures)
                            .FirstOrDefaultAsync(cr => cr.Id == fixture.CupRoundId.Value, ct);

                        if (cupRound == null)
                        {
                            _logger.LogWarning("CupRound with ID {CupRoundId} not found for fixture {FixtureId}",
                                fixture.CupRoundId.Value, fixture.Id);
                            break;
                        }

                        var allFinished = cupRound.Fixtures.All(f => f.WinnerTeamId != null);
                        if (allFinished)
                        {
                            await _cupFixturesService.GenerateNextRoundAsync(
                                cupRound.CupId,
                                fixture.GameSaveId,
                                fixture.SeasonId
                            );
                        }
                    }
                    break;

                case CompetitionType.EuropeanCup:
                    if (fixture.EuropeanCupPhaseId.HasValue)
                    {
                        var phase = await _context.EuropeanCupPhases
                            .Include(p => p.PhaseTemplate)
                            .FirstOrDefaultAsync(p => p.Id == fixture.EuropeanCupPhaseId.Value, ct);

                        if (phase?.PhaseTemplate?.IsKnockout == false)
                        {
                            await _eurocupStandingsService.UpdateEuropeanCupStandingsAfterMatchAsync(fixture.Id, ct);
                        }
                    }
                    break;
            }
        }
    }
}
