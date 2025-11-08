namespace TheDugout.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using TheDugout.Services;
    using TheDugout.Services.CPUManager;
    using TheDugout.Services.Cup;
    using TheDugout.Services.EuropeanCup;
    using TheDugout.Services.Facilities;
    using TheDugout.Services.Finance;
    using TheDugout.Services.Fixture;
    using TheDugout.Services.Game;
    using TheDugout.Services.League;
    using TheDugout.Services.Match;
    using TheDugout.Services.MatchEngine;
    using TheDugout.Services.Message;
    using TheDugout.Services.Player;
    using TheDugout.Services.Players;
    using TheDugout.Services.Season;
    using TheDugout.Services.Standings;
    using TheDugout.Services.Team;
    using TheDugout.Services.Template;
    using TheDugout.Services.Training;
    using TheDugout.Services.Transfer;
    using TheDugout.Services.User;
    using TheDugout.Services.GameSettings;
    using TheDugout.Services.Season.Interfaces;
    using TheDugout.Services.League.Interfaces;
    using TheDugout.Services.Cup.Interfaces;
    using TheDugout.Services.EuropeanCup.Interfaces;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Team.Interfaces;
    using TheDugout.Services.Match.Interfaces;
    using TheDugout.Services.Competition.Interfaces;
    using TheDugout.Services.Competition;
    using TheDugout.Services.Message.Interfaces;
    using Scrutor;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.GameSettings.Interfaces;
    using TheDugout.Services.Game.Interfaces;
    using TheDugout.Services.Staff.Interfaces;
    using TheDugout.Services.Training.Interfaces;
    using TheDugout.Services.Transfer.Interfaces;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDugoutServices(this IServiceCollection services)
        {
            // Game
            services.AddScoped<IGameSaveService, GameSaveService>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<ICPUManagerService, CpuManagerService>();

            // Gamesettings
            services.AddScoped<IGameSettingsService, GameSettingsService>();

            // Money Prizes
            services.AddScoped<IMoneyPrizeService, MoneyPrizeService>();

            // Season
            services.AddScoped<INewSeasonService, NewSeasonService>();
            services.AddScoped<IGameDayService, GameDayService>();
            services.AddScoped<INewSeasonService, NewSeasonService>();
            services.AddScoped<IEndSeasonService, EndSeasonService>();
            services.AddScoped<ISeasonEventService, SeasonEventService>();
            services.AddScoped<ISeasonCleanupService, SeasonCleanupService>();

            // Player
            services.AddScoped<IPlayerGenerationService, PlayerGenerationService>();
            services.AddScoped<IPlayerInfoService, PlayerInfoService>();
            services.AddScoped<IPlayerStatsService, PlayerStatsService>();
            services.AddScoped<IYouthPlayerService, YouthPlayerService>();
            services.AddScoped<IShortlistPlayerService, ShortlistPlayerService>();

            // Team
            services.AddScoped<ITeamPlanService, TeamPlanService>();
            services.AddScoped<ITeamGenerationService, TeamGenerationService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<ITeamFinanceService, TeamFinanceService>();

            // League
            services.AddScoped<ILeagueService, LeagueService>();
            services.AddScoped<ILeagueFixturesService, LeagueFixturesService>();
            services.AddScoped<ILeagueResultService, LeagueResultService>();
            services.AddScoped<ILeagueScheduleService, LeagueScheduleService>();
            services.AddScoped<ILeagueStandingsService, LeagueStandingsService>();

            //Transfers
            services.AddScoped<ITransferQueryService, TransferQueryService>();
            services.AddScoped<IFreeAgentTransferService, FreeAgentTransferService>();
            services.AddScoped<IClubToClubTransferService, ClubToClubTransferService>();

            // Finances
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<ITransactionService, TransactionService>();

            // Competitions
            services.AddScoped<ICompetitionService, CompetitionService>();
            services.AddScoped<IStandingsDispatcherService, StandingsDispatcherService>();

            // Eurocup
            services.AddScoped<IEuropeanCupService, EuropeanCupService>();
            services.AddScoped<IEuroCupTeamService, EuroCupTeamService>();
            services.AddScoped<IEurocupFixturesService, EurocupFixturesService>();
            services.AddScoped<IEurocupScheduleService, EurocupScheduleService>();
            services.AddScoped<IEurocupKnockoutService, EurocupKnockoutService>();
            services.AddScoped<IEuropeanCupStandingService, EuropeanCupStandingService>();
            services.AddScoped<IEuropeanCupResultService, EuropeanCupResultService>();

            // Cup
            services.AddScoped<ICupFixturesService, CupFixturesService>();
            services.AddScoped<ICupService, CupService>();
            services.AddScoped<ICupScheduleService, CupScheduleService>();
            services.AddScoped<ICupResultService, CupResultService>();

            // Training
            services.AddScoped<ITrainingService, TrainingService>();

            // Agency
            services.AddScoped<IAgencyService, AgencyService>();
            services.AddScoped<IAgencyFinanceService, AgencyFinanceService>();

            // Messages
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IMessageOrchestrator, MessageOrchestrator>();
            services.Scan(scan => scan
                .FromAssemblyOf<IMessagePlaceholderBuilder>()
                .AddClasses(classes => classes.AssignableTo<IMessagePlaceholderBuilder>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Fixtures
            services.AddScoped<IFixturesHelperService, FixturesHelperService>();

            // Matches
            services.AddScoped<IMatchService, MatchService>();
            services.AddScoped<IMatchEngine, MatchEngine>();
            services.AddScoped<IMatchEventService, MatchEventService>();
            services.AddScoped<IMatchResponseService, MatchResponseService>();
            services.AddScoped<IPenaltyShootoutService, PenaltyShootoutService>();

            // Facilities
            services.AddScoped<ITrainingFacilitiesService, TrainingFacilitiesService>();
            services.AddScoped<IYouthAcademyService, YouthAcademyService>();
            services.AddScoped<IStadiumService, StadiumService>();

            // ???
            services.AddScoped<ITemplateService, TemplateService>();                   
                                   

            return services;
        }
    }
}
