namespace TheDugout.Services.Player
{
    using EFCore.BulkExtensions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using TheDugout.Data;
    using TheDugout.DTOs.Player;
    using TheDugout.Models.Facilities;
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Services.Player.Interfaces;
    public class YouthPlayerService : IYouthPlayerService
    {
        private readonly DugoutDbContext _context;
        private readonly IPlayerGenerationService _playerGenerationService;
        private readonly ILogger<YouthPlayerService> _logger;
        private readonly Random _random;
        public YouthPlayerService(DugoutDbContext context, IPlayerGenerationService playerGenerationService, ILogger<YouthPlayerService> logger)
        {
            _context = context;
            _playerGenerationService = playerGenerationService;

            _random = new Random();
            _logger = logger;
        }
        public async Task GenerateAllYouthIntakesAsync(YouthAcademy academy, GameSave gameSave, CancellationToken ct = default)
        {
            if (academy == null)
                throw new ArgumentNullException(nameof(academy));
            if (academy.Team == null)
                throw new InvalidOperationException("Academy must have a team.");
            if (academy.Team.Country == null)
                throw new InvalidOperationException("Team must have a country.");

            var positions = await _context.Positions
                .AsNoTracking()
                .ToListAsync(ct);

            var players = new List<Player>();
            var youthPlayers = new List<YouthPlayer>();

            int level = academy.Level;
            int numberOfPlayers = Math.Min(5, level);
            var country = academy.Team.Country;

            for (int i = 0; i < numberOfPlayers; i++)
            {
                var position = positions[_random.Next(positions.Count)];
                var player = _playerGenerationService.CreateBasePlayer(gameSave, null, country, position, minAge: 15, maxAge: 17);

                player.CurrentAbility = _random.Next(level * 5, level * 10 + 1);
                player.PotentialAbility = _random.Next(level * 15, level * 20 + 1);

                players.Add(player);
            }

            await _context.BulkInsertAsync(players, new BulkConfig { SetOutputIdentity = true }, cancellationToken: ct);

            // След това:
            var allAttributes = players
                .SelectMany(p =>
                {
                    foreach (var attr in p.Attributes)
                        attr.PlayerId = p.Id; // много важно — след BulkInsert EF не знае PlayerId
                    return p.Attributes;
                })
                .ToList();

            await _context.BulkInsertAsync(allAttributes, cancellationToken: ct);

            foreach (var player in players)
            {
                youthPlayers.Add(new YouthPlayer
                {
                    PlayerId = player.Id,
                    YouthAcademyId = academy.Id,
                    IsPromoted = false,
                    GameSaveId = gameSave.Id
                });
            }

            await _context.BulkInsertAsync(youthPlayers, cancellationToken: ct);
        }
        public async Task<List<PlayerDto>> GetYouthPlayersByTeamAsync(int teamId)
        {
            if (teamId <= 0)
                throw new ArgumentException("Invalid team id.", nameof(teamId));

            var players = await _context.YouthPlayers
                .Include(y => y.Player)
                    .ThenInclude(p => p.Country)
                .Include(y => y.Player.Position)
                .Include(y => y.Player.Attributes)
                    .ThenInclude(a => a.Attribute)
                .Include(y => y.Player.SeasonStats)
                .Where(y => y.YouthAcademy.TeamId == teamId && !y.IsPromoted)
                .Select(y => new PlayerDto
                {
                    Id = y.Player.Id,
                    FullName = y.Player.FirstName + " " + y.Player.LastName,
                    Age = y.Player.Age,
                    Country = y.Player.Country != null ? y.Player.Country.Name : "Unknown",
                    Position = y.Player.Position != null ? y.Player.Position.Name : "N/A",
                    HeightCm = y.Player.HeightCm,
                    WeightKg = y.Player.WeightKg,
                    AvatarFileName = y.Player.AvatarFileName,

                    Attributes = y.Player.Attributes.Select(a => new PlayerAttributeDto
                    {
                        AttributeId = a.AttributeId,
                        Name = a.Attribute.Name,
                        Value = a.Value,
                        Category = a.Attribute.Category
                    }).ToList(),

                    SeasonStats = y.Player.SeasonStats.Select(s => new PlayerSeasonStatsDto
                    {
                        SeasonId = s.SeasonId,
                        MatchesPlayed = s.MatchesPlayed,
                        Goals = s.Goals
                    }).ToList()
                })
                .ToListAsync();

            return players;
        }

        public async Task<YouthPlayer?> GetYouthPlayerByIdAsync(int playerId)
        {
            if (playerId <= 0)
                throw new ArgumentException("Invalid player ID.", nameof(playerId));

            return await _context.YouthPlayers
                .Include(y => y.Player)
                    .ThenInclude(p => p.Country)
                .Include(y => y.Player.Position)
                .Include(y => y.YouthAcademy)
                .FirstOrDefaultAsync(y => y.PlayerId == playerId);
        }

        public async Task UpdateYouthPlayerAsync(YouthPlayer youthPlayer)
        {
            if (youthPlayer == null)
                throw new ArgumentNullException(nameof(youthPlayer));

            _context.YouthPlayers.Update(youthPlayer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteYouthPlayerAsync(int id)
        {
            var player = await _context.YouthPlayers
                .Include(y => y.Player)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (player != null)
            {
                // Изтриваме и Player, ако е обвързан директно само с академията
                if (player.Player != null)
                    _context.Players.Remove(player.Player);

                _context.YouthPlayers.Remove(player);
                await _context.SaveChangesAsync();
            }
        }

    }
}