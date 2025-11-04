namespace TheDugout.Services.Player
{
    using EFCore.BulkExtensions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using TheDugout.Data;
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
                var player = _playerGenerationService.CreateBasePlayer(gameSave, null, country, position, minAge:15,maxAge:17);

                player.CurrentAbility = _random.Next(level * 5, level * 10 + 1);
                player.PotentialAbility = _random.Next(level * 15, level * 20 + 1);

                players.Add(player);
            }

            // 1️⃣ Първо записваме играчите, за да получат реални ID-та
            await _context.BulkInsertAsync(players, new BulkConfig { SetOutputIdentity = true }, cancellationToken: ct);

            // 2️⃣ После генерираме youth записи с PlayerId
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


        public async Task<List<Player>> GetYouthPlayersByTeamAsync(int teamId)
        {
            if (teamId <= 0)
                throw new ArgumentException("Invalid team id.", nameof(teamId));

            var players = await _context.YouthPlayers
                .Include(y => y.Player)
                    .ThenInclude(p => p.Country)
                .Include(y => y.Player.Position)
                .Include(y => y.Player.SeasonStats)
                .Where(y => y.YouthAcademy.TeamId == teamId && !y.IsPromoted)
                .Select(y => y.Player)
                .ToListAsync();

            return players;
        }


    }
}