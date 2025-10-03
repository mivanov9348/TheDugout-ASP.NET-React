using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Game;
using TheDugout.Models.Teams;
using TheDugout.Services.Facilities;
using TheDugout.Services.Finance;
using TheDugout.Services.Players;
using TheDugout.Services.Team;
using TheDugout.Services.Training;

namespace TheDugout.Services.Team
{
    public class TeamGenerationService : ITeamGenerationService
    {
        private readonly IPlayerGenerationService _playerGenerator;
        private readonly IFinanceService _financeService;
        private readonly IStadiumService _stadiumService;
        private readonly ITrainingFacilitiesService _trainingService;
        private readonly IYouthAcademyService _academyService;
        private readonly DugoutDbContext _context;

        public TeamGenerationService(IPlayerGenerationService playerGenerator, IFinanceService financeService, DugoutDbContext context, IStadiumService stadiumService, ITrainingFacilitiesService trainingFacilitiesService, IYouthAcademyService youthAcademyService)
        {
            _playerGenerator = playerGenerator;
            _financeService = financeService;
            _context = context;
            _stadiumService = stadiumService;
            _trainingService = trainingFacilitiesService;
            _academyService = youthAcademyService;
        }

        public async Task<List<Models.Teams.Team>> GenerateTeamsAsync(
        GameSave gameSave,
        Models.Leagues.League league,
        IEnumerable<TeamTemplate> templates)
        {
            var teams = new List<Models.Teams.Team>();

            foreach (var tt in templates)
            {
                var team = new Models.Teams.Team
                {
                    TemplateId = tt.Id,
                    GameSave = gameSave,
                    GameSaveId = gameSave.Id,
                    League = league,
                    Name = tt.Name,
                    Abbreviation = tt.Abbreviation,
                    CountryId = tt.CountryId,
                    Country = tt.Country,
                    Popularity = 10,
                    LogoFileName = GenerateLogoFileName(tt.Name)
                };

                var players = _playerGenerator.GenerateTeamPlayers(gameSave, team);
                foreach (var player in players)
                {
                    gameSave.Players.Add(player);
                    team.Players.Add(player);
                }

                team.Popularity = CalculateTeamPopularity(team);

                teams.Add(team);
            }

            _context.Teams.AddRange(teams);
            await _context.SaveChangesAsync(); 

            foreach (var team in teams)
            {
                await _stadiumService.AddStadiumAsync(team.Id);
                await _trainingService.AddTrainingFacilityAsync(team.Id);
                await _academyService.AddYouthAcademyAsync(team.Id);
            }

            return teams;
        }
        private int CalculateTeamPopularity(Models.Teams.Team team)
        {
            if (team.Players == null || !team.Players.Any())
                return 10;

            var allValues = team.Players
                .SelectMany(p => p.Attributes)
                .Select(pa => pa.Value);

            double avgSkill = allValues.Any() ? allValues.Average() : 10;

            int popularity = 10 + (int)(avgSkill * 2);

            return Math.Clamp(popularity, 1, 100);
        }

        private decimal CalculateInitialFunds(int popularity)
        {
            const decimal baseAmount = 50_000m;
            decimal bonus = popularity * 1_000m;
            return baseAmount + bonus;
        }

        private string GenerateLogoFileName(string teamName)
        {

            var cleanName = System.Text.RegularExpressions.Regex.Replace(
                teamName,
                @"[<>:""/\\|?*]",
                ""
            );

            return $"{cleanName}.png";
        }

        public async Task<List<Models.Teams.Team>> GenerateIndependentTeamsAsync(GameSave gameSave)
        {
            var templates = await _context.TeamTemplates
                .Where(tt => tt.LeagueId == null) 
                .ToListAsync();

            var teams = new List<Models.Teams.Team>();

            foreach (var tt in templates)
            {
                var team = new Models.Teams.Team
                {
                    TemplateId = tt.Id,
                    GameSave = gameSave,
                    GameSaveId = gameSave.Id,
                    League = null, 
                    Name = tt.Name,
                    Abbreviation = tt.Abbreviation,
                    CountryId = tt.CountryId,
                    Country = tt.Country,
                    Popularity = 10,
                    LogoFileName = GenerateLogoFileName(tt.Name)
                };

                // Генерираме играчи
                var players = _playerGenerator.GenerateTeamPlayers(gameSave, team);
                foreach (var player in players)
                {
                    gameSave.Players.Add(player);
                    team.Players.Add(player);
                }

                team.Popularity = CalculateTeamPopularity(team);
                teams.Add(team);
            }

            _context.Teams.AddRange(teams);
            return teams;
        }


    }
}
