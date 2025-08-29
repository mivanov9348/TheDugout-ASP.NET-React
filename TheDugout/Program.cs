using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MySqlConnector;
using TheDugout.Services.User;
using TheDugout.Services.Template;
using TheDugout.Services.Game;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<DugoutDbContext>(options =>
options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IGameSaveService, GameSaveService>();

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
