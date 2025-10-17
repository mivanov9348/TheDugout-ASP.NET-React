

using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Enums;
using TheDugout.Models.Fixtures;
using TheDugout.Services.Cup.Interfaces;
using TheDugout.Services.EuropeanCup;
using TheDugout.Services.EuropeanCup.Interfaces;
using TheDugout.Services.League.Interfaces;

namespace TheDugout.Services.Standings
{
    public class StandingsDispatcherService : IStandingsDispatcherService
    {
        private readonly ILogger<StandingsDispatcherService> _logger;
        private readonly DugoutDbContext _context;
        private readonly ILeagueStandingsService _leagueStandingsService;
        private readonly ICupFixturesService _cupFixturesService;
        private readonly IEuropeanCupStandingService _eurocupStandingsService;
        private readonly IEuroCupTeamService _euroCupTeamService;
        private readonly IEuropeanCupService _europeanCupService;
        private readonly IEurocupKnockoutService _eurocupKnockoutService;

        public StandingsDispatcherService(ILogger<StandingsDispatcherService> logger, DugoutDbContext context, ILeagueStandingsService leagueStandingsService, ICupFixturesService cupFixturesService, IEuropeanCupStandingService eurocupStandingsService, IEurocupKnockoutService eurocupKnockoutService, IEuroCupTeamService euroCupTeamService, IEuropeanCupService europeanCupService)
        {
            _logger = logger;
            _context = context;
            _leagueStandingsService = leagueStandingsService;
            _cupFixturesService = cupFixturesService;
            _eurocupStandingsService = eurocupStandingsService;
            _eurocupKnockoutService = eurocupKnockoutService;
            _euroCupTeamService = euroCupTeamService;
            _europeanCupService = europeanCupService;
        }

        public async Task UpdateAfterMatchAsync(Models.Fixtures.Fixture fixture, CancellationToken ct = default)
        {

            switch (fixture.CompetitionType)
            {
                case CompetitionTypeEnum.League:
                    if (fixture.LeagueId.HasValue)
                        await _leagueStandingsService.UpdateStandingsAfterMatchAsync(fixture);
                    break;

                case CompetitionTypeEnum.DomesticCup:
                    if (fixture.CupRoundId.HasValue)
                    {
                        var cupRound = await _context.CupRounds
                                      .Include(cr => cr.Cup)
                                          .ThenInclude(c => c.Teams)
                                      .Include(cr => cr.Cup)
                                          .ThenInclude(c => c.Rounds)
                                              .ThenInclude(r => r.Fixtures)
                                      .Include(cr => cr.Fixtures)
                                      .FirstOrDefaultAsync(cr => cr.Id == fixture.CupRoundId.Value, ct);

                        if (cupRound.Cup.SeasonId != fixture.SeasonId)
                        {
                            _logger.LogWarning("Fixture {FixtureId} belongs to a cup from different season {CupSeasonId}",
                                fixture.Id, cupRound.Cup.SeasonId);
                            break;
                        }

                        if (cupRound == null)
                        {
                            _logger.LogWarning("CupRound with ID {CupRoundId} not found for fixture {FixtureId}",
                                fixture.CupRoundId.Value, fixture.Id);
                            break;
                        }

                        var allFinished = _cupFixturesService.IsRoundFinished(cupRound);

                        if (_cupFixturesService.IsRoundFinished(cupRound))
                        {
                            await _cupFixturesService.GenerateNextRoundAsync(
                                cupRound.CupId ?? -1,
                                fixture.GameSaveId,
                                fixture.SeasonId
                            );
                        }

                    }
                    break;

                case CompetitionTypeEnum.EuropeanCup:
                    if (fixture.EuropeanCupPhaseId.HasValue)
                    {
                        var phase = await _context.EuropeanCupPhases
    .Include(p => p.PhaseTemplate)
    .Include(p => p.EuropeanCup)
        .ThenInclude(c => c.Phases)
            .ThenInclude(p => p.PhaseTemplate)  
    .Include(p => p.EuropeanCup)
        .ThenInclude(c => c.Phases)
            .ThenInclude(p => p.Fixtures)
    .FirstOrDefaultAsync(p => p.Id == fixture.EuropeanCupPhaseId.Value, ct);

                        if (phase?.PhaseTemplate?.IsKnockout == false)
                        {
                            await _eurocupStandingsService.UpdateEuropeanCupStandingsAfterMatchAsync(fixture.Id, ct);

                            if (_eurocupStandingsService.AreAllGroupMatchesPlayed(phase.EuropeanCup))
                            {
                                await _eurocupKnockoutService.DeterminePostGroupAdvancementAsync(phase.EuropeanCupId);
                                await _eurocupKnockoutService.GeneratePlayoffRoundAsync(phase.EuropeanCupId);
                            }

                        }
                        else
                        {
                            bool allMatchesFinished = phase.Fixtures.All(f => f.Status == FixtureStatusEnum.Played);

                            if (allMatchesFinished)
                            {
                                bool isFinalPhase = !phase.EuropeanCup.Phases
                                    .Any(p => p.PhaseTemplate.Order > phase.PhaseTemplate.Order);

                                if (isFinalPhase)
                                {
                                    var finalMatch = phase.Fixtures
                                        .OrderByDescending(f => f.Date)
                                        .FirstOrDefault();

                                    if (finalMatch != null)
                                        await _europeanCupService.HandleFinalMatchCompletionAsync(phase.EuropeanCupId, finalMatch, ct);
                                }
                                else
                                {
                                    await _eurocupKnockoutService.GenerateNextKnockoutPhaseAsync(
                                        phase.EuropeanCupId,
                                        phase.PhaseTemplate.Order
                                    );
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
