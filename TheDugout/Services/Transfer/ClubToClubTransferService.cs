namespace TheDugout.Services.Transfer
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Transfer;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Teams;
    using TheDugout.Models.Transfers;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.GameSettings;
    using TheDugout.Services.GameSettings.Interfaces;
    using TheDugout.Services.Message.Interfaces;
    using TheDugout.Services.Transfer.Interfaces;

    public class ClubToClubTransferService : IClubToClubTransferService
    {
        private readonly DugoutDbContext _context;
        private readonly ITeamFinanceService _teamFinanceService;
        private readonly IMessageOrchestrator _messageOrchestrator;
        private readonly IGameSettingsService _gameSettingsService;

        public ClubToClubTransferService(
            DugoutDbContext context,
            ITeamFinanceService teamFinanceService,
            IMessageOrchestrator messageOrchestrator,
            IGameSettingsService gameSettingsService)
        {
            _context = context;
            _teamFinanceService = teamFinanceService;
            _messageOrchestrator = messageOrchestrator;
            _gameSettingsService = gameSettingsService;
        }

        public async Task<(bool Success, string ErrorMessage)> SendOfferAsync(TransferOfferRequest request)
        {
            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == request.PlayerId && p.GameSaveId == request.GameSaveId);
            if (player == null) return (false, "Player not found.");
            if (player.TeamId == null) return (false, "Player is a free agent, use normal buy method.");

            var fromTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.FromTeamId);
            var toTeam = await _context.Teams
                .Include(t => t.GameSave)
                .FirstOrDefaultAsync(t => t.Id == request.ToTeamId);

            if (fromTeam == null || toTeam == null) return (false, "Invalid teams.");
            if (request.OfferAmount <= 0) return (false, "Invalid offer.");

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.GameSaveId == request.GameSaveId &&
                                           s.IsActive &&
                                          s.StartDate <= DateTime.UtcNow &&
                                          s.EndDate >= DateTime.UtcNow);

            if (season == null) return (false, "No active season found.");

            bool inTransferWindow = season.Events.Any(e => e.Type == SeasonEventType.TransferWindow && e.Date.Date == season.CurrentDate.Date);
            if (!inTransferWindow) return (false, "Outside transfer window.");

            // 🧠 ПРОВЕРКА: дали продавачът ще остане с достатъчно играчи
            bool canSell = await SellerHasEnoughPlayersAsync(toTeam.Id, player.Position);
            if (!canSell)
                return (false, $"{toTeam.Name} cannot sell {player.FirstName} {player.LastName} — not enough players left on that position.");

            bool sellerIsCpu = toTeam.GameSave.UserTeamId != toTeam.Id;

            if (sellerIsCpu)
            {
                bool accepted = await CpuDecideToSell(player, fromTeam, toTeam, request.OfferAmount);
                if (!accepted)
                    return (false, $"{toTeam.Name} rejected the offer for {player.FirstName} {player.LastName}.");

                var (success, err) = await FinalizeTransferAsync(request.GameSaveId, fromTeam, toTeam, player, season, request.OfferAmount);
                return (success, err);
            }
            else
            {
                var offer = new TransferOffer
                {
                    GameSaveId = request.GameSaveId,
                    SeasonId = season.Id,
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

        private async Task<bool> CpuDecideToSell(Player player, Team buyer, Team seller, decimal offer)
        {
            // 🚫 CPU не може да продава, ако ще остане без достатъчно играчи
            bool canSell = await SellerHasEnoughPlayersAsync(seller.Id, player.Position);
            if (!canSell)
                return false;

            decimal ratio = offer / player.Price;
            double baseChance = ratio switch
            {
                >= 1.2m => 0.9,
                >= 1.0m => 0.5,
                _ => 0.1
            };

            if (player.CurrentAbility > 80 && seller.Popularity > 70)
                baseChance *= 0.6;

            return new Random().NextDouble() <= baseChance;
        }

        private async Task<(bool, string)> FinalizeTransferAsync(
            int gameSaveId,
            Team buyer,
            Team seller,
            Player player,
            Season season,
            decimal fee)
        {
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);
            if (bank == null)
                return (false, "Bank not found.");

            var (ok, err) = await _teamFinanceService.ClubToClubWithFeeAsync(
                buyer,
                seller,
                bank,
                fee,
                $"Transfer of {player.FirstName} {player.LastName}");

            if (!ok)
                return (false, err);

            player.TeamId = buyer.Id;

            var transfer = new Transfer
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

            var fullTransfer = await _context.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromTeam)
                .Include(t => t.ToTeam)
                .FirstAsync(t => t.Id == transfer.Id);

            await _messageOrchestrator.SendMessageAsync(
                MessageCategory.Transfer,
                gameSaveId,
                fullTransfer);

            return (true, "");
        }

        private async Task<bool> SellerHasEnoughPlayersAsync(int teamId, Position position)
        {
            var teamPlayers = await _context.Players
                .Include(p => p.Position)
                .Where(p => p.TeamId == teamId)
                .ToListAsync();

            string posCode = position.Code.ToLower();

            int count = posCode switch
            {
                var s when s.Contains("gk") => teamPlayers.Count(p => p.Position.Code.Contains("GK")),
                var s when s.Contains("def") => teamPlayers.Count(p => p.Position.Code.Contains("DEF")),
                var s when s.Contains("mid") => teamPlayers.Count(p => p.Position.Code.Contains("MID")),
                var s when s.Contains("att") || s.Contains("st") => teamPlayers.Count(p => p.Position.Code.Contains("ATT") || p.Position.Code.Contains("ST")),
                _ => 0
            };

            int minNeeded = posCode switch
            {
                var s when s.Contains("gk") => await _gameSettingsService.GetIntAsync("startingGoalkeepers") ?? 1,
                var s when s.Contains("def") => await _gameSettingsService.GetIntAsync("startingDefenders") ?? 4,
                var s when s.Contains("mid") => await _gameSettingsService.GetIntAsync("startingMidfielders") ?? 4,
                var s when s.Contains("att") || s.Contains("st") => await _gameSettingsService.GetIntAsync("startingAttackers") ?? 2,
                _ => 1
            };

            return count - 1 >= minNeeded;
        }

    }
}
