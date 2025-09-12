using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services.Finance;
using TheDugout.Services.Players;
using TheDugout.Services.Team;

namespace TheDugout.Services.Team
{
    public class TeamGenerationService : ITeamGenerationService
    {
        private readonly IPlayerGenerationService _playerGenerator;
        private readonly IFinanceService _financeService;
        private readonly DugoutDbContext _context;

        public TeamGenerationService(IPlayerGenerationService playerGenerator, IFinanceService financeService, DugoutDbContext context)
        {
            _playerGenerator = playerGenerator;
            _financeService = financeService;
            _context = context;
        }

        public List<Models.Team> GenerateTeams(GameSave gameSave, Models.League league, IEnumerable<TeamTemplate> templates)
        {
            var teams = new List<Models.Team>();

            foreach (var tt in templates)
            {
                var team = new Models.Team
                {
                    TemplateId = tt.Id,
                    GameSave = gameSave,
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
                 

            return teams;
        }
        private int CalculateTeamPopularity(Models.Team team)
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

    }
}
