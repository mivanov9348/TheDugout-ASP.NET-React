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

        public async Task<List<Player>> GetShortlistPlayersAsync(int gameSaveId, int? userId = null, int? teamId = null)
        {
            var query = _context.Shortlists
                .Where(s => s.GameSaveId == gameSaveId)
                .Include(s => s.Player)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(s => s.UserId == userId);

            if (teamId.HasValue)
                query = query.Where(s => s.TeamId == teamId);

            return await query.Select(s => s.Player).ToListAsync();
        }


    }
}
