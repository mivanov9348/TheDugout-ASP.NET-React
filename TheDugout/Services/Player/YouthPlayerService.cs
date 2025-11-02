namespace TheDugout.Services.Player
{
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
        private readonly Random _random;
        public YouthPlayerService(DugoutDbContext context, IPlayerGenerationService playerGenerationService)
        {
            _context = context;
            _playerGenerationService = playerGenerationService;
            _random = new Random();
        }
        public async Task GenerateYouthIntakeAsync(YouthAcademy academy, GameSave save)
        {
            if (academy == null)
                throw new ArgumentNullException(nameof(academy));

            if (academy.Team == null)
                throw new InvalidOperationException("Academy needs a team!");

            int level = academy.Level;
            int numberOfPlayers = Math.Min(5, level);

            var country = academy.Team.Country
                ?? throw new InvalidOperationException("Team needs to have a Country.");

            // Асинхронно извличане на позиции от базата
            var positions = await _context.Positions.ToListAsync();

            for (int i = 0; i < numberOfPlayers; i++)
            {
                var position = positions[_random.Next(positions.Count)];

                var player = _playerGenerationService.CreateBasePlayer(save, null, country, position);

                player.CurrentAbility = _random.Next(level * 5, level * 10 + 1);
                player.PotentialAbility = _random.Next(level * 15, level * 20 + 1);

                var youth = new YouthPlayer
                {
                    Player = player,
                    YouthAcademy = academy,
                    IsPromoted = false
                };

                academy.YouthPlayers.Add(youth);

                await _context.AddAsync(player);
                await _context.AddAsync(youth);
            }

            await _context.SaveChangesAsync();
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