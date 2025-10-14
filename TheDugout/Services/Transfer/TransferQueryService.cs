namespace TheDugout.Services.Transfer
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;

    public class TransferQueryService : ITransferQueryService
    {
        private readonly DugoutDbContext _context;

        public TransferQueryService(DugoutDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetPlayersAsync(
            int gameSaveId,
            string? search,
            string? team,
            string? country,
            string? position,
            bool freeAgent,
            string sortBy,
            string sortOrder,
            int page,
            int pageSize,
            int? minAge = null,
            int? maxAge = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            var query = _context.Players.AsNoTracking()
                .Where(p => p.GameSaveId == gameSaveId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => (p.FirstName + " " + p.LastName).ToLower().Contains(search.ToLower()));

            if (!string.IsNullOrWhiteSpace(team))
                query = query.Where(p => p.Team != null && p.Team.Name.ToLower().Contains(team.ToLower()));

            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(p => p.Country != null && p.Country.Name.ToLower().Contains(country.ToLower()));

            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(p => p.Position != null && p.Position.Name.ToLower().Contains(position.ToLower()));

            if (freeAgent)
                query = query.Where(p => p.TeamId == null);

            if (maxAge.HasValue)
                query = query.Where(p => p.BirthDate >= DateTime.Now.AddYears(-maxAge.Value));

            if (minAge.HasValue)
                query = query.Where(p => p.BirthDate <= DateTime.Now.AddYears(-minAge.Value));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            query = sortBy.ToLower() switch
            {
                "team" => sortOrder == "desc" ? query.OrderByDescending(p => p.Team!.Name) : query.OrderBy(p => p.Team!.Name),
                "position" => sortOrder == "desc" ? query.OrderByDescending(p => p.Position!.Name) : query.OrderBy(p => p.Position!.Name),
                "country" => sortOrder == "desc" ? query.OrderByDescending(p => p.Country!.Name) : query.OrderBy(p => p.Country!.Name),
                "age" => sortOrder == "desc" ? query.OrderByDescending(p => p.Age) : query.OrderBy(p => p.Age),
                "price" => sortOrder == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                _ => sortOrder == "desc" ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName)
            };

            var totalCount = await query.CountAsync();
            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    Name = p.FirstName + " " + p.LastName,
                    Team = p.Team != null ? p.Team.Name : "",
                    TeamId = p.TeamId,
                    Country = p.Country != null ? p.Country.Name : "",
                    Position = p.Position != null ? p.Position.Name : "",
                    p.Age,
                    p.Price
                })
                .ToListAsync();

            return new { TotalCount = totalCount, Page = page, PageSize = pageSize, Players = players };
        }

        public async Task<IEnumerable<object>> GetTransferHistoryAsync(int gameSaveId, bool onlyMine)
        {
            var gameSave = await _context.GameSaves
                .Include(gs => gs.UserTeam)
                .FirstOrDefaultAsync(gs => gs.Id == gameSaveId);
            if (gameSave == null) return Enumerable.Empty<object>();

            var query = _context.Transfers
                .AsNoTracking()
                .Include(t => t.Player)
                .Include(t => t.FromTeam)
                .Include(t => t.ToTeam)
                .Include(t => t.Season)
                .Where(t => t.GameSaveId == gameSaveId);

            if (onlyMine && gameSave.UserTeamId != null)
            {
                int myTeamId = gameSave.UserTeamId.Value;
                query = query.Where(t => t.FromTeamId == myTeamId || t.ToTeamId == myTeamId);
            }

            return await query.OrderByDescending(t => t.GameDate)
                .Select(t => new
                {
                    t.Id,
                    Player = t.Player.FirstName + " " + t.Player.LastName,
                    FromTeam = t.FromTeam != null ? t.FromTeam.Name : (t.IsFreeAgent ? "Free Agent" : ""),
                    ToTeam = t.ToTeam != null ? t.ToTeam.Name : "",
                    t.Fee,
                    t.GameDate,
                    t.IsFreeAgent,
                    Season = t.Season != null ? $"{t.Season.StartDate.Year}/{t.Season.EndDate.Year}" : ""
                })
                .ToListAsync();
        }
    }
}
