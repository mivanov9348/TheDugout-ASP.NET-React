namespace TheDugout.Services.Players
{
    using Bogus;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Team;

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

        public List<Player> GenerateTeamPlayers(GameSave save, Team team)
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
                    string selectedPositionCode = positionCode;
                    if (positionCode == "ANY")
                    {
                        string[] options = { "GK", "DF", "MID", "ATT" };
                        selectedPositionCode = options[_rng.Next(options.Length)];
                    }

                    var position = _context.Positions.FirstOrDefault(p => p.Code == selectedPositionCode);
                    if (position == null)
                        throw new InvalidOperationException($"Position with code '{selectedPositionCode}' not found in database.");

                    var randomCountry = countries[_rng.Next(countries.Count)];
                    if (randomCountry == null)
                        throw new InvalidOperationException("Selected random country is null.");

                    var player = CreateBasePlayer(save, team, randomCountry, position);
                    players.Add(player);
                }
            }

            return players;
        }

        public List<Player> GenerateFreeAgents(GameSave save, int count = 100)
        {
            if (save == null) throw new ArgumentNullException(nameof(save), "GameSave cannot be null.");
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

            var countries = _context.Countries.ToList();
            var positions = _context.Positions.ToList();

            if (!countries.Any())
                throw new InvalidOperationException("No countries found in the database.");
            if (!positions.Any())
                throw new InvalidOperationException("No positions found in the database.");

            var players = new List<Player>();

            for (int i = 0; i < count; i++)
            {
                var randomCountry = countries[_rng.Next(countries.Count)];
                var position = positions[_rng.Next(positions.Count)];

                var player = CreateBasePlayer(save, null, randomCountry, position);
                players.Add(player);
            }

            return players;
        }

        private Player CreateBasePlayer(GameSave save, Team? team, Country country, Position position)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (country == null) throw new ArgumentNullException(nameof(country));
            if (position == null) throw new ArgumentNullException(nameof(position));

            var regionCode = country.RegionCode;
            if (string.IsNullOrWhiteSpace(regionCode))
                throw new InvalidOperationException($"Country {country.Code} няма RegionCode.");

            var firstNames = _context.FirstNames
                .Where(fn => fn.RegionCode == regionCode)
                .Select(fn => fn.Name)
                .ToList();

            var lastNames = _context.LastNames
                .Where(ln => ln.RegionCode == regionCode)
                .Select(ln => ln.Name)
                .ToList();

            if (!firstNames.Any())
                throw new InvalidOperationException($"No First Names For {regionCode}");
            if (!lastNames.Any())
                throw new InvalidOperationException($"No First Names For {regionCode}");

            var firstName = firstNames[_rng.Next(firstNames.Count)];
            var lastName = lastNames[_rng.Next(lastNames.Count)];

            var player = new Player
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = RandomBirthDate(),
                Team = team,
                GameSave = save,
                Position = position,
                HeightCm = _rng.Next(165, 200),
                WeightKg = _rng.Next(65, 95),
                IsActive = true,
                Country = country,
                Attributes = new List<PlayerAttribute>()
            };

            AssignAttributes(player, position);
            player.Price = CalculatePlayerPrice(player);

            return player;
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

                    int value = GenerateRealisticAttribute(w.Weight, age);

                    player.Attributes.Add(new PlayerAttribute
                    {
                        AttributeId = w.AttributeId,
                        Value = value
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

        private int GenerateRealisticAttribute(double weight, int age)
        {
            double baseRandom = 5 + _rng.NextDouble() * 15; // между 5 и 20
            double weighted = baseRandom * (0.5 + weight);

            double ageFactor = age switch
            {
                <= 20 => 0.6 + (age - 16) * 0.1,
                <= 24 => 1.0,
                <= 28 => 1.2,
                <= 32 => 1.0,
                <= 35 => 0.8,
                _ => 0.6
            };

            double variation = 0.9 + _rng.NextDouble() * 0.2;
            int final = (int)Math.Round(weighted * ageFactor * variation);

            return Math.Clamp(final, 1, 20);
        }

        private DateTime RandomBirthDate()
        {
            int age = _rng.Next(18, 36);
            var today = DateTime.Today;
            return today.AddYears(-age).AddDays(-_rng.Next(0, 365));
        }

        private decimal CalculatePlayerPrice(Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            double avgAttribute = player.Attributes.Any()
                ? player.Attributes.Average(a => a.Value)
                : 10;

            double ageFactor = player.Age switch
            {
                < 20 => 0.8,
                <= 23 => 1.0,
                <= 28 => 1.3,
                <= 32 => 1.1,
                _ => 0.7
            };

            double positionFactor = player.Position.Code switch
            {
                "ATT" => 1.5,
                "MID" => 1.3,
                "DF" => 1.1,
                "GK" => 0.9,
                _ => 1.0
            };

            double basePrice = avgAttribute * 2000;
            double price = basePrice * ageFactor * positionFactor;

            double variation = 0.85 + (_rng.NextDouble() * 0.3);
            price *= variation;

            if (price > 120_000) price = 120_000;

            return Math.Round((decimal)price, 0);
        }

    }
}
