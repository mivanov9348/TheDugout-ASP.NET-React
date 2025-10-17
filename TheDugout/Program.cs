using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheDugout.Data;
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
using TheDugout.Services.Staff;
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

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<DugoutDbContext>(options =>
    options.UseSqlServer(connectionString, opt => opt.CommandTimeout(90)));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services DI
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IGameSaveService, GameSaveService>();
builder.Services.AddScoped<IPlayerGenerationService, PlayerGenerationService>();
builder.Services.AddScoped<ITeamPlanService, TeamPlanService>();
builder.Services.AddScoped<ITeamGenerationService, TeamGenerationService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<INewSeasonService, NewSeasonService>();
builder.Services.AddScoped<IPlayerInfoService, PlayerInfoService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<ITeamFinanceService, TeamFinanceService>();
builder.Services.AddScoped<IAgencyFinanceService, AgencyFinanceService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<ITransferQueryService, TransferQueryService>();
builder.Services.AddScoped<IClubToClubTransferService, ClubToClubTransferService>();
builder.Services.AddScoped<IFreeAgentTransferService, FreeAgentTransferService>();
builder.Services.AddScoped<IEuropeanCupService, EuropeanCupService>();
builder.Services.AddScoped<IEuroCupTeamService, EuroCupTeamService>();
builder.Services.AddScoped<ICupService, CupService>();
builder.Services.AddScoped<IAgencyService, AgencyService>();
builder.Services.AddScoped<IFixturesHelperService, FixturesHelperService>();
builder.Services.AddScoped<ILeagueFixturesService, LeagueFixturesService>();
builder.Services.AddScoped<ICupFixturesService, CupFixturesService>();
builder.Services.AddScoped<IEurocupFixturesService, EurocupFixturesService>();
builder.Services.AddScoped<ILeagueScheduleService, LeagueScheduleService>();
builder.Services.AddScoped<ICupScheduleService, CupScheduleService>();
builder.Services.AddScoped<IEurocupScheduleService, EurocupScheduleService>();
builder.Services.AddScoped<IEurocupKnockoutService, EurocupKnockoutService>();
builder.Services.AddScoped<ITrainingFacilitiesService, TrainingFacilitiesService>();
builder.Services.AddScoped<IYouthAcademyService, YouthAcademyService>();
builder.Services.AddScoped<IStadiumService, StadiumService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMessageOrchestrator, MessageOrchestrator>();
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IMessagePlaceholderBuilder>() 
    .AddClasses(classes => classes.AssignableTo<IMessagePlaceholderBuilder>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddScoped<IGameDayService, GameDayService>();
builder.Services.AddScoped<ISeasonEventService, SeasonEventService>();
builder.Services.AddScoped<ICPUManagerService, CpuManagerService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IMatchEngine, MatchEngine>();
builder.Services.AddScoped<IMatchEventService, MatchEventService>();
builder.Services.AddScoped<IPlayerStatsService, PlayerStatsService>();
builder.Services.AddScoped<ILeagueStandingsService, LeagueStandingsService>();
builder.Services.AddScoped<IStandingsDispatcherService, StandingsDispatcherService>();
builder.Services.AddScoped<IEuropeanCupStandingService, EuropeanCupStandingService>();
builder.Services.AddScoped<IPenaltyShootoutService, PenaltyShootoutService>();
builder.Services.AddScoped<IGameSettingsService, GameSettingsService>();
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<INewSeasonService, NewSeasonService>();
builder.Services.AddScoped<IEndSeasonService, EndSeasonService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });   

});

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Чети токен от query string (SignalR) или от cookie
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/game"))
                {
                    context.Token = accessToken;
                }
                else
                {
                    context.Token = context.Request.Cookies["jwt"];
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Ако искаш в текстов файл:
builder.Logging.AddFile("Logs/app-{Date}.txt");

var app = builder.Build();

await TheDugout.Infrastructure.SeedData.EnsureSeededAsync(app.Services, app.Logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
