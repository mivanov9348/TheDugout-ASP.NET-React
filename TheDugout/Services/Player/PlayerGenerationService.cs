using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using TheDugout.Data;
using TheDugout.Models;
using TheDugout.Services.Team;

namespace TheDugout.Services.Players
{
    public class PlayerGenerationService : IPlayerGenerationService
    {
        private readonly ITeamPlanService _teamPlan;
        private readonly DugoutDbContext _context; // твоя DbContext
        private readonly Random _rng = new();

        public PlayerGenerationService(ITeamPlanService teamPlan, DugoutDbContext context)
        {
            _teamPlan = teamPlan;
            _context = context;
        }

        public List<Player> GenerateTeamPlayers(GameSave save, Models.Team team)
        {
            var players = new List<Player>();
            var plan = _teamPlan.GetDefaultRosterPlan();

            foreach (var kv in plan)
            {
                var positionCode = kv.Key;
                var count = kv.Value;

                for (int i = 0; i < count; i++)
                {
                    if (positionCode == "ANY")
                    {
                        string[] options = { "GK", "DF", "MID", "ATT" };
                        positionCode = options[_rng.Next(options.Length)];
                    }

                    var position = _context.Positions.First(p => p.Code == positionCode);

                    var locale = GetLocaleForCountry(team.Country.Code);
                    var faker = new Faker(locale);

                    var player = new Player
                    {
                        FirstName = faker.Name.FirstName(),
                        LastName = faker.Name.LastName(),
                        BirthDate = RandomBirthDate(),
                        Team = team,
                        GameSave = save,
                        Position = position,
                        HeightCm = _rng.Next(165, 200),
                        WeightKg = _rng.Next(65, 95),
                        IsActive = true,
                        Country = team.Country 
                    };

                    players.Add(player);
                }
            }

            return players;
        }

        private DateTime RandomBirthDate()
        {
            int age = _rng.Next(18, 36);
            var today = DateTime.Today;
            return today.AddYears(-age).AddDays(-_rng.Next(0, 365));
        }

        private string GetLocaleForCountry(string countryCode)
        {
            return countryCode switch
            {
                "ALG" => "ar",   
                "FRA" => "fr",   
                "ESP" => "es",   
                "GER" => "de",  
                "ITA" => "it",   
                "JPN" => "ja",
                "BUL" => "bg",
                "CRO" => "hr",
                "CZE" => "cs",
                "HUN" => "hu",
                "NED" => "nl",
                "POL" => "pl",
                "POR" => "pt",
                "ROU" => "ro",
                "RUS" => "ru",
                "SRB" => "sr",
                "TUR" => "tr",
        
                _ => "en"
            };
        }
    }
}
