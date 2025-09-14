using Microsoft.EntityFrameworkCore;
using TheDugout.Data;
using TheDugout.Models.Finance;
using TheDugout.Models.Seasons;
using TheDugout.Services.Finance;

namespace TheDugout.Services.Transfer
{
    public class TransferService : ITransferService
    {
        private readonly DugoutDbContext _context;
        private readonly IFinanceService _financeService;

        public TransferService(DugoutDbContext context, IFinanceService _financeService)
        {
            _context = context;
            this._financeService = _financeService;
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
            int pageSize)
        {
            var query = _context.Players
                .AsNoTracking()
                .Include(p => p.Team)
                .Include(p => p.Position)
                .Include(p => p.Country)
                .Include(p => p.Attributes).ThenInclude(pa => pa.Attribute)
                .Include(p => p.SeasonStats)
                .Where(p => p.GameSaveId == gameSaveId)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.ToLower();
                query = query.Where(p => p.FirstName.ToLower().Contains(lowered) ||
                                         p.LastName.ToLower().Contains(lowered));
            }

            // Team filter
            if (!string.IsNullOrWhiteSpace(team))
                query = query.Where(p => p.Team != null &&
                                         p.Team.Name.ToLower().Contains(team.ToLower()));

            // Country filter
            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(p => p.Country != null &&
                                         p.Country.Name.ToLower().Contains(country.ToLower()));

            // Position filter
            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(p => p.Position != null &&
                                         p.Position.Name.ToLower().Contains(position.ToLower()));

            // Free agent filter
            if (freeAgent)
                query = query.Where(p => p.TeamId == null);

            // Sorting
            query = sortBy.ToLower() switch
            {
                "team" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Team != null).ThenByDescending(p => p.Team!.Name)
                    : query.OrderBy(p => p.Team != null).ThenBy(p => p.Team!.Name),
                "position" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Position.Name)
                    : query.OrderBy(p => p.Position.Name),
                "country" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Country!.Name)
                    : query.OrderBy(p => p.Country!.Name),
                "age" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Age)
                    : query.OrderBy(p => p.Age),
                "price" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "freeagent" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.TeamId == null)
                    : query.OrderBy(p => p.TeamId == null),
                _ => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.LastName)
                    : query.OrderBy(p => p.LastName),
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
                    Country = p.Country != null ? p.Country.Name : "",
                    Position = p.Position != null ? p.Position.Name : "",
                    p.Age,
                    p.Price,
                    Attributes = _context.Attributes
                        .Select(attr => new
                        {
                            attr.Code,
                            attr.Name,
                            Value = p.Attributes
                                .Where(pa => pa.AttributeId == attr.Id)
                                .Select(pa => pa.Value)
                                .FirstOrDefault()
                        })
                        .ToList(),
                    SeasonStats = p.SeasonStats.Select(s => new
                    {
                        s.SeasonId,
                        s.Goals,
                        s.Assists,
                        s.MatchesPlayed
                    }).ToList()
                })
                .ToListAsync();

            return new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Players = players
            };
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
                .Where(t => t.GameSaveId == gameSaveId)
                .AsQueryable();

            if (onlyMine && gameSave.UserTeamId != null)
            {
                int myTeamId = gameSave.UserTeamId.Value;
                query = query.Where(t => t.FromTeamId == myTeamId || t.ToTeamId == myTeamId);
            }

            return await query
                .OrderByDescending(t => t.GameDate)
                .Select(t => new
                {
                    t.Id,
                    Player = t.Player.FirstName + " " + t.Player.LastName,
                    FromTeam = t.FromTeam != null ? t.FromTeam.Name : (t.IsFreeAgent ? "Free Agent" : ""),
                    ToTeam = t.ToTeam != null ? t.ToTeam.Name : "",
                    t.Fee,
                    t.GameDate,
                    t.IsFreeAgent,
                    Season = t.Season != null ? t.Season.StartDate.Year + "/" + t.Season.EndDate.Year : ""
                })
                .ToListAsync();
        }
        public async Task<(bool Success, string ErrorMessage)> BuyPlayerAsync(int gameSaveId, int teamId, int playerId)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId && t.GameSaveId == gameSaveId);
            if (team == null)
                return (false, "Team not found.");

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == playerId && p.GameSaveId == gameSaveId);
            if (player == null)
                return (false, "Player not found.");

            if (player.TeamId != null)
                return (false, "Player already has a team.");

            var bank = await _context.Banks
                .FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);
            if (bank == null)
                return (false, "Bank not found for this save.");

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId &&
                                           s.StartDate <= DateTime.UtcNow &&
                                           s.EndDate >= DateTime.UtcNow);
            if (season == null)
                return (false, "No active season found for this save.");

            bool inTransferWindow = season.Events.Any(e =>
                e.Type == SeasonEventType.TransferWindow &&
                e.Date.Date == season.CurrentDate.Date);

            if (!inTransferWindow)
                return (false, "Transfers are not allowed outside of the transfer window.");

            var transfer = new Models.Transfers.Transfer
            {
                GameSaveId = gameSaveId,
                SeasonId = season.Id,
                PlayerId = player.Id,
                FromTeamId = player.TeamId,
                ToTeamId = team.Id,
                Fee = player.Price,
                IsFreeAgent = player.TeamId == null,
                GameDate = season.CurrentDate
            };

            _context.Transfers.Add(transfer);

            var tx = await _financeService.ClubToBankAsync(
                team,
                bank,
                player.Price,
                $"Transfer fee for {player.FirstName} {player.LastName}",
                TransactionType.TransferFee
            );

            if (tx.Status != TransactionStatus.Completed)
                return (false, "Payment failed: " + tx.Status);

            player.TeamId = team.Id;

            await _context.SaveChangesAsync();

            return (true, "");
        }


    }
}
