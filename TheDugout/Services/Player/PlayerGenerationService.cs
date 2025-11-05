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
        private record CachedCountry(int Id, string Code, string RegionCode);
        private record CachedPosition(string Code, int Id);

        private List<CachedCountry> _cachedCountries;
        private Dictionary<string, CachedPosition> _cachedPositions;
        private Dictionary<string, List<string>> _cachedFirstNamesByRegion;
        private Dictionary<string, List<string>> _cachedLastNamesByRegion;

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
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (team == null) throw new ArgumentNullException(nameof(team));

            var plan = _teamPlan.GetDefaultRosterPlan();
            if (plan == null || plan.Count == 0)
                throw new InvalidOperationException("Roster plan is null or empty.");

            // ✅ Кешираме само чисти DTO-та, не EF entities
            _cachedCountries ??= _context.Countries
                .AsNoTracking()
                .Select(c => new CachedCountry(c.Id, c.Code, c.RegionCode))
                .ToList();

            _cachedPositions ??= _context.Positions
                .AsNoTracking()
                .Select(p => new CachedPosition(p.Code, p.Id))
                .ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

            _cachedFirstNamesByRegion ??= _context.FirstNames
                .AsNoTracking()
                .GroupBy(fn => fn.RegionCode)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

            _cachedLastNamesByRegion ??= _context.LastNames
                .AsNoTracking()
                .GroupBy(ln => ln.RegionCode)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

            var players = new List<Player>(plan.Values.Sum());
            var anyOptions = new[] { "GK", "DF", "MID", "ATT" };

            foreach (var (positionCode, count) in plan)
            {
                if (string.IsNullOrWhiteSpace(positionCode))
                    throw new InvalidOperationException("Position code in roster plan is null or empty.");
                if (count <= 0)
                    throw new InvalidOperationException($"Invalid player count for position '{positionCode}'.");

                for (int i = 0; i < count; i++)
                {
                    string selectedPositionCode = positionCode == "ANY"
                        ? anyOptions[_rng.Next(anyOptions.Length)]
                        : positionCode;

                    if (!_cachedPositions.TryGetValue(selectedPositionCode, out var cachedPos))
                        throw new InvalidOperationException($"Position '{selectedPositionCode}' not found.");

                    var position = _context.Positions.Find(cachedPos.Id);

                    var cachedCountry = team.Country != null && _rng.NextDouble() < 0.8
                        ? new CachedCountry(team.Country.Id, team.Country.Code, team.Country.RegionCode)
                        : _cachedCountries[_rng.Next(_cachedCountries.Count)];

                    var country = _context.Countries.Find(cachedCountry.Id);

                    players.Add(CreateBasePlayer(save, team, country, position));
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
                var freeAgents = new List<Player>();

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
        public Player CreateBasePlayer(GameSave save, Team? team, Country country, Position position,
    Agency? agency = null, int? minAge = null, int? maxAge = null)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (country == null) throw new ArgumentNullException(nameof(country));
            if (position == null) throw new ArgumentNullException(nameof(position));

            var regionCode = country.RegionCode;
            if (string.IsNullOrWhiteSpace(regionCode))
                throw new InvalidOperationException($"Country {country.Code} няма RegionCode.");

            // --- 🔥 Lazy кеширане на имената (само при първи достъп) ---
            if (_cachedFirstNamesByRegion == null || _cachedLastNamesByRegion == null)
            {
                _cachedFirstNamesByRegion = _context.FirstNames.AsNoTracking()
                    .GroupBy(fn => fn.RegionCode)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

                _cachedLastNamesByRegion = _context.LastNames.AsNoTracking()
                    .GroupBy(ln => ln.RegionCode)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());
            }

            if (!_cachedFirstNamesByRegion.TryGetValue(regionCode, out var firstNames) || firstNames.Count == 0)
                throw new InvalidOperationException($"No First Names For {regionCode}");
            if (!_cachedLastNamesByRegion.TryGetValue(regionCode, out var lastNames) || lastNames.Count == 0)
                throw new InvalidOperationException($"No Last Names For {regionCode}");

            var firstName = firstNames[_rng.Next(firstNames.Count)];
            var lastName = lastNames[_rng.Next(lastNames.Count)];

            DateTime birthDate;
            if (minAge.HasValue && maxAge.HasValue)
                birthDate = RandomBirthDateWithinAgeRange(save.CurrentSeason.CurrentDate, minAge.Value, maxAge.Value);
            else
                birthDate = RandomBirthDate();

            var player = new Player
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                Team = team,
                TeamId = team?.Id,
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

        private DateTime RandomBirthDateWithinAgeRange(DateTime currentDate, int minAge, int maxAge)
        {
            var minBirthDate = currentDate.AddYears(-maxAge); 
            var maxBirthDate = currentDate.AddYears(-minAge); 
            var range = (maxBirthDate - minBirthDate).Days;
            return minBirthDate.AddDays(_rng.Next(range));
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

            int attributeCount = 20; 
            int maxAttributeValue = 20;
            int maxSum = attributeCount * maxAttributeValue; 
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

            player.CurrentAbility = (int)Math.Clamp(Math.Round(sum * scaleFactor), 1, 200);

            player.PotentialAbility = CalculatePotentialAbility(player, position);
        }
        private int GenerateWeightedAttribute(double weight, double totalWeight, int age)
        {
            double baseValue = 5 + _rng.NextDouble() * 15;
            double importanceFactor = 0.5 + (weight / totalWeight) * 1.5; 

            double ageFactor = age switch
            {
                <= 20 => 0.7 + _rng.NextDouble() * 0.4, 
                <= 24 => 0.9 + _rng.NextDouble() * 0.3,
                <= 28 => 1.1 + _rng.NextDouble() * 0.2,
                <= 32 => 1.0,
                <= 35 => 0.8 + _rng.NextDouble() * 0.2,
                _ => 0.6 + _rng.NextDouble() * 0.2
            };

            double final = baseValue * importanceFactor * ageFactor;
            return Math.Clamp((int)Math.Round(final), 1, 20);
        }

        public void UpdateCurrentAbility(Player player, PlayerSeasonStats stats)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            double potentialGap = player.PotentialAbility - player.CurrentAbility;
            if (potentialGap <= 0) potentialGap = 1; 

            double growth = 0.0;

            if (player.Age <= 25)
            {
                double matchFactor = Math.Min(stats.MatchesPlayed / 40.0, 1.0);
                double performanceFactor = (stats.SeasonRating - 6.5) * 0.15; // 6.5 е средно
                growth = potentialGap * (0.05 * matchFactor + performanceFactor * 0.05);
            }

            else if (player.Age <= 30)
            {
                double stability = Math.Clamp((stats.SeasonRating - 6.5) * 0.03, -0.02, 0.05);
                growth = player.CurrentAbility * stability;
            }
            else
            {
                double decline = _rng.NextDouble() * 0.3; 
                growth = -decline;
            }

            int newAbility = (int)Math.Clamp(player.CurrentAbility + growth, 1, player.PotentialAbility);
            player.CurrentAbility = newAbility;
        }
        private int CalculatePotentialAbility(Player player, Position position)
        {
            int maxGrowth = player.Age switch
            {
                <= 18 => _rng.Next(40, 80),
                <= 21 => _rng.Next(30, 60),
                <= 25 => _rng.Next(15, 40),
                <= 28 => _rng.Next(5, 20),
                _ => _rng.Next(0, 10)
            };

            double posFactor = position.Code switch
            {
                "ATT" => 1.1,
                "MID" => 1.05,
                "DF" => 1.0,
                "GK" => 0.9,  
                _ => 1.0
            };

            int potential = (int)(player.CurrentAbility + maxGrowth * posFactor);

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
        public async Task UpdatePlayerPriceAsync(Player player)
        {
            var stats = await _context.PlayerSeasonStats
                .Where(s => s.PlayerId == player.Id)
                .OrderByDescending(s => s.SeasonId)
                .FirstOrDefaultAsync();

            player.Price = CalculateUpdatedPlayerPrice(player, stats);

            _context.Update(player);
            await _context.SaveChangesAsync();
        }
        private decimal CalculateUpdatedPlayerPrice(Player player, PlayerSeasonStats? stats)
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

            if (stats != null && stats.MatchesPlayed > 0)
            {
                double performanceFactor = 1.0;

                double ratingEffect = (stats.SeasonRating - 6.5) * 0.2;
                ratingEffect = Math.Clamp(ratingEffect, -0.3, 0.4);

                double goalImpact = player.Position.Code switch
                {
                    "ATT" => stats.Goals * 0.03,
                    "MID" => stats.Goals * 0.015,
                    "DF" => stats.Goals * 0.01,
                    _ => 0
                };

                double consistency = Math.Min(stats.MatchesPlayed / 40.0, 1.0);

                performanceFactor += (ratingEffect + goalImpact) * consistency;

              
  
                price *= performanceFactor;
            }

            double variation = 0.95 + (_rng.NextDouble() * 0.1);
            price *= variation;

            double smoothed = ((double)player.Price * 0.7) + (price * 0.3);

            smoothed = Math.Clamp(smoothed, 5000, 120_000);

            return Math.Round((decimal)smoothed, 0);
        }
        public string GetRandomAvatarFileName()
        {
            var randomFile = _avatarFiles[_rng.Next(_avatarFiles.Length)];
            return Path.GetFileName(randomFile);
        }
    }
}
