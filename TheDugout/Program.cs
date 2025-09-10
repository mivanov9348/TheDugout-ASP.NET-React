using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TheDugout.Data;
using TheDugout.Services;
using TheDugout.Services.Finance;
using TheDugout.Services.Fixture;
using TheDugout.Services.Game;
using TheDugout.Services.Interfaces;
using TheDugout.Services.League;
using TheDugout.Services.Players;
using TheDugout.Services.Season;
using TheDugout.Services.Team;
using TheDugout.Services.Template;
using TheDugout.Services.Training;
using TheDugout.Services.Transfer;
using TheDugout.Services.User;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<DugoutDbContext>(options =>
options.UseSqlServer(connectionString, opt => opt.CommandTimeout(60)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IGameSaveService, GameSaveService>();
builder.Services.AddScoped<IPlayerGenerationService, PlayerGenerationService>();
builder.Services.AddScoped<ITeamPlanService, TeamPlanService>();
builder.Services.AddScoped<IFixturesService, FixturesService>();
builder.Services.AddScoped<ITeamGenerationService, TeamGenerationService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<ISeasonGenerationService, SeasonGenerationService>();
builder.Services.AddScoped<IPlayerInfoService, PlayerInfoService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<ITransferService, TransferService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // frontend port
              .AllowAnyHeader()
              .AllowAnyMethod()
        .AllowCredentials();
    });
});

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

        // Чети token от cookie вместо header (custom)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["jwt"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});


builder.Services.AddAuthorization();

var app = builder.Build();

await TheDugout.Infrastructure.SeedData.EnsureSeededAsync(app.Services, app.Logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
