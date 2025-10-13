namespace TheDugout.Services.Transfer
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Transfer;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Transfers;
    using TheDugout.Services.Finance;
    using TheDugout.Services.Message;

    public class TransferService : ITransferService
    {
        private readonly DugoutDbContext _context;
        private readonly IFinanceService _financeService;
        private readonly IMessageOrchestrator _messageOrchestrator;
        private readonly ILogger<TransferService> _logger = null!;

        public TransferService(DugoutDbContext context, IFinanceService _financeService, IMessageOrchestrator messageOrchestrator)
        {
            _context = context;
            this._financeService = _financeService;
            _messageOrchestrator = messageOrchestrator;
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
            var query = _context.Players
                .AsNoTracking()
                .Where(p => p.GameSaveId == gameSaveId)
                .AsQueryable();

            // ✅ Филтри по текст
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(search.ToLower()) ||
                    p.LastName.ToLower().Contains(search.ToLower()));

            if (!string.IsNullOrWhiteSpace(team))
                query = query.Where(p => p.Team != null &&
                                         p.Team.Name.ToLower().Contains(team.ToLower()));

            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(p => p.Country != null &&
                                         p.Country.Name.ToLower().Contains(country.ToLower()));

            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(p => p.Position != null &&
                                         p.Position.Name.ToLower().Contains(position.ToLower()));

            if (freeAgent)
                query = query.Where(p => p.TeamId == null);

            // ✅ Нови диапазони
            if (maxAge.HasValue)
            {
                var maxBirthDate = DateTime.Now.AddYears(-maxAge.Value);
                query = query.Where(p => p.BirthDate >= maxBirthDate);
            }

            if (minAge.HasValue)
            {
                var minBirthDate = DateTime.Now.AddYears(-minAge.Value);
                query = query.Where(p => p.BirthDate <= minBirthDate);
            }

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // ✅ Сортиране
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
                    Country = p.Country != null ? p.Country.Name : "",
                    Position = p.Position != null ? p.Position.Name : "",
                    p.Age,
                    p.Price
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

            await _messageOrchestrator.SendMessageAsync(
    MessageCategory.Transfer,
    gameSaveId,
    transfer
);

            return (true, "");
        }

        public async Task<(bool Success, string ErrorMessage)> SendOfferAsync(TransferOfferRequest request)
        {
            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == request.PlayerId && p.GameSaveId == request.GameSaveId);

            if (player == null)
                return (false, "Player not found.");

            if (player.TeamId == null)
                return (false, "Player is a free agent, use normal buy method.");

            if (player.TeamId != request.ToTeamId)
                return (false, "Player does not belong to that team.");

            if (request.OfferAmount <= 0)
                return (false, "Invalid offer amount.");

            var fromTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.FromTeamId);
            var toTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.ToTeamId);

            if (fromTeam == null || toTeam == null)
                return (false, "Invalid teams.");

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.GameSaveId == request.GameSaveId &&
                                          s.StartDate <= DateTime.UtcNow &&
                                          s.EndDate >= DateTime.UtcNow);
            if (season == null)
                return (false, "No active season found.");

            bool inTransferWindow = season.Events.Any(e =>
                e.Type == SeasonEventType.TransferWindow &&
                e.Date.Date == season.CurrentDate.Date);

            if (!inTransferWindow)
                return (false, "Transfers are not allowed outside the transfer window.");

            // Ако е CPU отбор → AI решава
            bool sellerIsCpu = toTeam.GameSave.UserTeamId != toTeam.Id;
            if (sellerIsCpu)
            {
                bool accepted = await CpuDecideToSell(player, fromTeam, toTeam, request.OfferAmount);
                if (!accepted)
                    return (false, $"{toTeam.Name} rejected the offer for {player.FirstName} {player.LastName}.");

                // Ако приеме — правим реален трансфер
                var (success, err) = await FinalizeTransferAsync(request.GameSaveId, fromTeam, toTeam, player, season, request.OfferAmount);
                if (!success)
                    return (false, err);

                return (true, $"Transfer completed: {player.FirstName} {player.LastName} -> {fromTeam.Name}");
            }
            else
            {
                var offer = new TransferOffer
                {
                    GameSaveId = request.GameSaveId,
                    FromTeamId = request.FromTeamId,
                    ToTeamId = request.ToTeamId,
                    PlayerId = request.PlayerId,
                    OfferAmount = request.OfferAmount,
                    CreatedAt = DateTime.UtcNow,
                    Status = OfferStatus.Pending
                };

                _context.TransferOffers.Add(offer);
                await _context.SaveChangesAsync();

                return (true, "Offer sent and pending user decision.");
            }
        }
        private async Task<bool> CpuDecideToSell(Models.Players.Player player, Models.Teams.Team buyer, Models.Teams.Team seller, decimal offer)
        {
            // Базова логика:
            // - ако офертата е >= 120% от цената → 90% шанс да приеме
            // - ако е 100–119% → 50% шанс
            // - ако е под 100% → 10% шанс
            // - ако играчът е твърде ценен (висок ability) и seller е богат → по-малък шанс
            decimal ratio = offer / player.Price;
            double baseChance = ratio switch
            {
                >= 1.2m => 0.9,
                >= 1.0m => 0.5,
                _ => 0.1
            };

            // Корекция според важността на играча
            if (player.CurrentAbility > 80 && seller.Popularity > 70)
                baseChance *= 0.6;

            // Малко случайност
            var random = new Random();
            return random.NextDouble() <= baseChance;
        }
        private async Task<(bool, string)> FinalizeTransferAsync(
    int gameSaveId,
    Models.Teams.Team buyer,
    Models.Teams.Team seller,
    Models.Players.Player player,
    Season season,
    decimal fee)
        {
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);
            if (bank == null) return (false, "Bank not found.");

            var tx = await _financeService.ClubToBankAsync(
                buyer,
                bank,
                fee,
                $"Transfer fee for {player.FirstName} {player.LastName}",
                TransactionType.TransferFee
            );

            if (tx.Status != TransactionStatus.Completed)
                return (false, "Payment failed.");

            player.TeamId = buyer.Id;

            var transfer = new Models.Transfers.Transfer
            {
                GameSaveId = gameSaveId,
                SeasonId = season.Id,
                PlayerId = player.Id,
                FromTeamId = seller.Id,
                ToTeamId = buyer.Id,
                Fee = fee,
                IsFreeAgent = false,
                GameDate = season.CurrentDate
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            await _messageOrchestrator.SendMessageAsync(MessageCategory.Transfer, gameSaveId, transfer);

            return (true, "");
        }


        public async Task RunCpuTransfersAsync(int gameSaveId, int seasonId, DateTime date, int teamId)
        {
            try
            {
                // 1. Провери дали е трансферен прозорец
                var season = await _context.Seasons
                    .Include(s => s.Events)
                    .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.Id == seasonId);

                var team = await _context.Teams
                    .FirstOrDefaultAsync(t => t.GameSaveId == gameSaveId && t.Id == teamId);

                if (team == null) return;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Проблем с трансфери на отбор {TeamId}", teamId);
            }
        }


    }
}
