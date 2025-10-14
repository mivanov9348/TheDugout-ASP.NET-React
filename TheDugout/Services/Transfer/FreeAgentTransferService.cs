namespace TheDugout.Services.Transfer
{
    using Microsoft.EntityFrameworkCore;
    using TheDugout.Data;
    using TheDugout.Models.Enums;
    using TheDugout.Models.Finance;
    using TheDugout.Models.Messages;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Transfers;
    using TheDugout.Services.Finance;
    using TheDugout.Services.Message;

    public class FreeAgentTransferService : IFreeAgentTransferService
    {
        private readonly DugoutDbContext _context;
        private readonly IFinanceService _financeService;
        private readonly IMessageOrchestrator _messageOrchestrator;

        public FreeAgentTransferService(DugoutDbContext context, IFinanceService financeService, IMessageOrchestrator messageOrchestrator)
        {
            _context = context;
            _financeService = financeService;
            _messageOrchestrator = messageOrchestrator;
        }

        public async Task<(bool Success, string ErrorMessage)> SignFreePlayer(int gameSaveId, int teamId, int playerId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.GameSaveId == gameSaveId);
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId && p.GameSaveId == gameSaveId);
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);

            if (team == null) return (false, "Team not found.");
            if (player == null) return (false, "Player not found.");
            if (player.TeamId != null) return (false, "Player already has a team.");
            if (bank == null) return (false, "Bank not found.");

            var season = await _context.Seasons.Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId &&
                                           s.StartDate <= DateTime.UtcNow &&
                                           s.EndDate >= DateTime.UtcNow);

            if (season == null) return (false, "No active season found.");
            bool inTransferWindow = season.Events.Any(e =>
                e.Type == SeasonEventType.TransferWindow &&
                e.Date.Date == season.CurrentDate.Date);

            if (!inTransferWindow) return (false, "Transfers are not allowed outside of the window.");

            var tx = await _financeService.ClubToBankAsync(team, bank, player.Price,
                $"Transfer fee for {player.FirstName} {player.LastName}", TransactionType.TransferFee);

            if (tx.Status != TransactionStatus.Completed)
                return (false, "Payment failed: " + tx.Status);

            player.TeamId = team.Id;

            var transfer = new Transfer
            {
                GameSaveId = gameSaveId,
                SeasonId = season.Id,
                PlayerId = player.Id,
                ToTeamId = team.Id,
                Fee = player.Price,
                IsFreeAgent = true,
                GameDate = season.CurrentDate
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            await _messageOrchestrator.SendMessageAsync(MessageCategory.Transfer, gameSaveId, transfer);

            return (true, "");
        }
    }
}
