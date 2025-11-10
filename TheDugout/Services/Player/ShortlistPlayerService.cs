namespace TheDugout.Services.Player
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Players;
    using TheDugout.Services.Player.Interfaces;
    public class ShortlistPlayerService : IShortlistPlayerService
    {
        private readonly DugoutDbContext _context;
        public ShortlistPlayerService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task AddToShortlistAsync(int gameSaveId, int playerId, int? userId = null, int? teamId = null, string? note = null)
        {
            var exists = await _context.Shortlists
                .AnyAsync(s => s.GameSaveId == gameSaveId && s.PlayerId == playerId && (s.UserId == userId || s.TeamId == teamId));

            if (exists)
                return;

            var entry = new Shortlist
            {
                GameSaveId = gameSaveId,
                PlayerId = playerId,
                UserId = userId,
                TeamId = teamId,
                Note = note
            };

            _context.Shortlists.Add(entry);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromShortlistAsync(int gameSaveId, int playerId, int? userId = null, int? teamId = null)
        {
            var entry = await _context.Shortlists
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.PlayerId == playerId && (s.UserId == userId || s.TeamId == teamId));

            if (entry != null)
            {
                _context.Shortlists.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<object>> GetShortlistPlayersAsync(int gameSaveId, int? userId = null, int? teamId = null)
        {
            var query = _context.Shortlists
                .Where(s => s.GameSaveId == gameSaveId)
                .Include(s => s.Player)
                    .ThenInclude(p => p.Position)
                .Include(s => s.Player)
                    .ThenInclude(p => p.Team)
                .Include(s => s.Player)
                    .ThenInclude(p => p.Country)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(s => s.UserId == userId);

            if (teamId.HasValue)
                query = query.Where(s => s.TeamId == teamId);

            var players = await query
                .Select(s => new
                {
                    s.Player.Id,
                    s.Player.FirstName,
                    s.Player.LastName,
                    s.Player.Age,
                    Position = s.Player.Position != null ? s.Player.Position.Name : null,
                    TeamName = s.Player.Team != null ? s.Player.Team.Name : null,
                    Country = s.Player.Country != null ? s.Player.Country.Name : null,
                    s.Player.Price,
                    s.Player.CurrentAbility,
                    s.Player.PotentialAbility,
                    s.Player.HeightCm,
                    s.Player.WeightKg
                })
                .ToListAsync();

            return players.Cast<object>().ToList();
        }



    }
}
