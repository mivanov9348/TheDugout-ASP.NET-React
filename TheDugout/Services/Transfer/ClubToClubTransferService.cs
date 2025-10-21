namespace TheDugout.Services.Transfer
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.DTOs.Transfer;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Transfers;
    using TheDugout.Services.Finance.Interfaces;
    using TheDugout.Services.Message.Interfaces;

    public class ClubToClubTransferService : IClubToClubTransferService
    {
        private readonly DugoutDbContext _context;
        private readonly ITeamFinanceService _teamFinanceService;
        private readonly IMessageOrchestrator _messageOrchestrator;

        public ClubToClubTransferService(
            DugoutDbContext context,
            ITeamFinanceService teamFinanceService,
            IMessageOrchestrator messageOrchestrator)
        {
            _context = context;
            _teamFinanceService = teamFinanceService;
            _messageOrchestrator = messageOrchestrator;
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
                                          s.StartDate <= DateTime.UtcNow &&
                                          s.EndDate >= DateTime.UtcNow);

            if (season == null) return (false, "No active season found.");

            bool inTransferWindow = season.Events.Any(e => e.Type == SeasonEventType.TransferWindow && e.Date.Date == season.CurrentDate.Date);
            if (!inTransferWindow) return (false, "Outside transfer window.");

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

        private async Task<bool> CpuDecideToSell(
            Models.Players.Player player,
            Models.Teams.Team buyer,
            Models.Teams.Team seller,
            decimal offer)
        {
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
            Models.Teams.Team buyer,
            Models.Teams.Team seller,
            Models.Players.Player player,
            Season season,
            decimal fee)
        {
            var bank = await _context.Banks
                .FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);
            if (bank == null)
                return (false, "Bank not found.");

            // 💸 1️⃣ Финансова операция с 10% такса за банката
            var (ok, err) = await _teamFinanceService.ClubToClubWithFeeAsync(
                buyer,
                seller,
                bank,
                fee,
                $"Transfer of {player.FirstName} {player.LastName}");

            if (!ok)
                return (false, err);

            // ⚽ 2️⃣ Прехвърляме играча към новия клуб
            player.TeamId = buyer.Id;

            // 📋 3️⃣ Записваме трансфера
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
    }
}
