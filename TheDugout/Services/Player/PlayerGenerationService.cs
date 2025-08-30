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
        private readonly DugoutDbContext _context;
        private readonly Random _rng = new();

        public PlayerGenerationService(ITeamPlanService teamPlan, DugoutDbContext context)
        {
            _teamPlan = teamPlan;
            _context = context;
        }

        public List<Player> GenerateTeamPlayers(GameSave save, Models.Team team)
        {
            try
            {
                if (save == null)
                    throw new ArgumentNullException(nameof(save), "GameSave cannot be null.");
                if (team == null)
                    throw new ArgumentNullException(nameof(team), "Team cannot be null.");

                var players = new List<Player>();
                var plan = _teamPlan.GetDefaultRosterPlan();

                if (plan == null || plan.Count == 0)
                    throw new InvalidOperationException("Roster plan is null or empty.");

                var countries = _context.Countries.ToList();
                if (countries == null || countries.Count == 0)
                    throw new InvalidOperationException("No countries found in the database.");

                foreach (var kv in plan)
                {
                    var positionCode = kv.Key;
                    var count = kv.Value;

                    if (string.IsNullOrWhiteSpace(positionCode))
                        throw new InvalidOperationException("Position code in roster plan is null or empty.");
                    if (count <= 0)
                        throw new InvalidOperationException($"Invalid player count for position '{positionCode}'.");

                    for (int i = 0; i < count; i++)
                    {
                        if (positionCode == "ANY")
                        {
                            string[] options = { "GK", "DF", "MID", "ATT" };
                            positionCode = options[_rng.Next(options.Length)];
                        }

                        var position = _context.Positions.FirstOrDefault(p => p.Code == positionCode);
                        if (position == null)
                            throw new InvalidOperationException($"Position with code '{positionCode}' not found in database.");

                        var randomCountry = countries[_rng.Next(countries.Count)];
                        if (randomCountry == null || randomCountry.Code == null)
                            throw new InvalidOperationException("Selected random country or its code is null.");

                        var locale = GetLocaleForCountry(randomCountry.Code);
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
                            Country = randomCountry,
                            Attributes = new List<PlayerAttribute>()
                        };

                        AssignAttributes(player, position);

                        players.Add(player);
                    }
                }

                return players;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR in GenerateTeamPlayers] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw; // хвърляме пак, за да не скрием грешката
            }
        }

        private void AssignAttributes(Player player, Position position)
        {
            try
            {
                if (player == null)
                    throw new ArgumentNullException(nameof(player), "Player cannot be null.");
                if (position == null)
                    throw new ArgumentNullException(nameof(position), "Position cannot be null.");

                int age = DateTime.Today.Year - player.BirthDate.Year;
                if (player.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;

                var weights = _context.PositionWeights
                    .Include(w => w.Attribute)
                    .Where(w => w.PositionId == position.Id)
                    .ToList();

                if (weights == null || weights.Count == 0)
                    throw new InvalidOperationException($"No position weights found for position '{position.Code}'.");

                foreach (var w in weights)
                {
                    if (w.AttributeId <= 0)
                        throw new InvalidOperationException("Invalid AttributeId in PositionWeight.");
                    if (w.Weight < 0)
                        throw new InvalidOperationException("Weight in PositionWeight cannot be negative.");

                    double expected = 20 * w.Weight;
                    double ageFactor = AgeFactor(age);
                    double noise = _rng.NextDouble() * 4 - 2;
                    double rawValue = expected * ageFactor + noise;

                    int finalValue = Math.Clamp((int)Math.Round(rawValue), 1, 20);

                    player.Attributes.Add(new PlayerAttribute
                    {
                        AttributeId = w.AttributeId,
                        Value = finalValue
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR in AssignAttributes] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private double AgeFactor(int age)
        {
            if (age < 21) return 0.7 + (age - 18) * 0.07;
            if (age <= 27) return 0.9 + (age - 21) * 0.03;
            if (age <= 30) return 1.1 - (age - 27) * 0.03;
            if (age <= 34) return 1.0 - (age - 30) * 0.04;
            return 0.8;
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
                "BUL" => "ru",
                "NED" => "nl",
                "POL" => "pl",
                "POR" => "pt_PT",
                "ROU" => "ro",
                "RUS" => "ru",
                "TUR" => "tr",
                _ => "en"
            };
        }
    }
}
