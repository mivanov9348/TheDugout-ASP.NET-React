namespace TheDugout.Services.Players
{
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheDugout.Data;
    using TheDugout.Models.Common;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Staff;
    using TheDugout.Models.Teams;
    using TheDugout.Services.Player.Interfaces;
    using TheDugout.Services.Team.Interfaces;

    public class PlayerGenerationService : IPlayerGenerationService
    {
        private readonly ITeamPlanService _teamPlan;
        private readonly DugoutDbContext _context;
        private readonly Random _rng = new();

        private readonly string[] _avatarFiles;
        private readonly string _avatarFolder;

        public PlayerGenerationService(ITeamPlanService teamPlan, DugoutDbContext context, IWebHostEnvironment env)
        {
            _teamPlan = teamPlan;
            _context = context;

            // Използваме WebRootPath (wwwroot), ако го няма – fallback към ContentRootPath/wwwroot
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            _avatarFolder = Path.Combine(webRoot, "Avatars");

            if (!Directory.Exists(_avatarFolder))
            {
                Directory.CreateDirectory(_avatarFolder); // вместо да гърми -> създаваме папката
            }

            _avatarFiles = Directory.GetFiles(_avatarFolder);

            if (_avatarFiles.Length == 0)
            {
                throw new InvalidOperationException($"No avatar files found in {_avatarFolder}");
            }
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

                    Country selectedCountry;
                    if (team != null && team.Country != null && countries.Any())
                    {
                        if (_rng.NextDouble() < 0.8)
                        {
                            selectedCountry = team.Country;
                        }
                        else
                        {
                            selectedCountry = countries[_rng.Next(countries.Count)];
                        }
                    }
                    else
                    {
                        selectedCountry = countries[_rng.Next(countries.Count)];
                    }

                    if (selectedCountry == null)
                        throw new InvalidOperationException("Selected country is null.");

                    var player = CreateBasePlayer(save, team, selectedCountry, position);
                    players.Add(player);
                }
            }

            return players;
        }

        public Player? GenerateFreeAgent(GameSave save, Agency agency)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (agency == null) throw new ArgumentNullException(nameof(agency));

            var countries = _context.Countries.ToList();
            var positions = _context.Positions.ToList();

            if (!countries.Any() || !positions.Any())
                return null;

            var position = positions[_rng.Next(positions.Count)];
            var country = countries[_rng.Next(countries.Count)];

            var player = CreateBasePlayer(save, null, country, position, agency);

            if (player.Price > agency.Budget)
                return null;

            agency.Budget -= player.Price;

            return player;
        }

        public async Task GeneratePlayersForAgenciesAsync(GameSave save, List<Agency> agencies, CancellationToken ct = default)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (agencies == null || agencies.Count == 0) return;

            foreach (var agency in agencies)
            {
                var freeAgents = new List<Models.Players.Player>();

                while (true)
                {
                    var player = GenerateFreeAgent(save, agency);
                    if (player == null)
                        break;

                    freeAgents.Add(player);
                    _context.Players.Add(player);
                }
            }

            await _context.SaveChangesAsync(ct);
        }


        private Player CreateBasePlayer(GameSave save, Team? team, Country country, Position position, Agency? agency = null)
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
                TeamId = team?.Id,
                GameSave = save,
                GameSaveId = save.Id,
                Position = position,
                HeightCm = _rng.Next(165, 200),
                WeightKg = _rng.Next(65, 95),
                IsActive = true,
                Country = country,
                Attributes = new List<PlayerAttribute>(),
                Agency = agency,
                AvatarFileName = GetRandomAvatarFileName()
            };

            AssignAttributes(player, position);
            player.Price = CalculatePlayerPrice(player);

            return player;
        }
        private void AssignAttributes(Player player, Position position)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            int age = player.Age;

            var weights = _context.PositionWeights
                .Include(w => w.Attribute)
                .Where(w => w.PositionId == position.Id)
                .ToList();

            if (weights == null || weights.Count == 0)
                throw new InvalidOperationException($"No position weights found for position '{position.Code}'.");

            int attributeCount = 20; // фиксирано, ако е различно → вземи от DB
            int maxAttributeValue = 20;
            int maxSum = attributeCount * maxAttributeValue; // 400 при 20 атрибута
            double scaleFactor = 200.0 / maxSum; // 0.5 при 20 атрибута

            int sum = 0;

            foreach (var w in weights)
            {
                int value = GenerateWeightedAttribute(w.Weight, weights.Sum(x => x.Weight), age);

                player.Attributes.Add(new PlayerAttribute
                {
                    AttributeId = w.AttributeId,
                    GameSaveId = player.GameSaveId,
                    Value = value
                });

                sum += value;
            }

            // CurrentAbility = сбор, скалиран към 200
            player.CurrentAbility = (int)Math.Clamp(Math.Round(sum * scaleFactor), 1, 200);

            // PotentialAbility = CA + възраст + позиционен фактор
            player.PotentialAbility = CalculatePotentialAbility(player, position);
        }
        private int GenerateWeightedAttribute(double weight, double totalWeight, int age)
        {
            // базово разпределение – играчите с важен атрибут за позицията да получават по-високи стойности
            double baseValue = 5 + _rng.NextDouble() * 15; // 5–20
            double importanceFactor = 0.5 + (weight / totalWeight) * 1.5; // по-висока тежест = по-голям шанс за висок атрибут

            // възрастов фактор (млади са по-непостоянни, пик 24–28, спад след 32)
            double ageFactor = age switch
            {
                <= 20 => 0.7 + _rng.NextDouble() * 0.4, // непостоянни, някои добри, други слаби
                <= 24 => 0.9 + _rng.NextDouble() * 0.3,
                <= 28 => 1.1 + _rng.NextDouble() * 0.2,
                <= 32 => 1.0,
                <= 35 => 0.8 + _rng.NextDouble() * 0.2,
                _ => 0.6 + _rng.NextDouble() * 0.2
            };

            // финална стойност
            double final = baseValue * importanceFactor * ageFactor;
            return Math.Clamp((int)Math.Round(final), 1, 20);
        }
        private int CalculatePotentialAbility(Player player, Position position)
        {
            int growthMargin = player.Age switch
            {
                <= 19 => _rng.Next(60, 100),
                <= 23 => _rng.Next(30, 70),
                <= 27 => _rng.Next(15, 40),
                <= 30 => _rng.Next(5, 20),
                _ => 0
            };

            double posFactor = position.Code switch
            {
                "ATT" => 1.15,
                "MID" => 1.05,
                "DF" => 1.0,
                "GK" => 0.95,
                _ => 1.0
            };

            int potential = (int)(player.CurrentAbility + growthMargin * posFactor);

            return Math.Min(potential, 200);
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

            double abilityScore = (player.CurrentAbility * 0.7) + (player.PotentialAbility * 0.3);

            double ageFactor = player.Age switch
            {
                < 20 => 1.3,   
                <= 23 => 1.2,
                <= 28 => 1.0, 
                <= 32 => 0.8,
                _ => 0.5      
            };

            double positionFactor = player.Position.Code switch
            {
                "ATT" => 1.4,
                "MID" => 1.2,
                "DF" => 1.0,
                "GK" => 0.8,
                _ => 1.0
            };

            double basePrice = (abilityScore / 200.0) * 100_000;

            double price = basePrice * ageFactor * positionFactor;

            double variation = 0.9 + (_rng.NextDouble() * 0.2); // 0.9–1.1
            price *= variation;

            if (price > 120_000) price = 120_000;

            return Math.Round((decimal)price, 0);
        }
        public string GetRandomAvatarFileName()
        {
            var randomFile = _avatarFiles[_rng.Next(_avatarFiles.Length)];
            return Path.GetFileName(randomFile);
        }
    }
}
