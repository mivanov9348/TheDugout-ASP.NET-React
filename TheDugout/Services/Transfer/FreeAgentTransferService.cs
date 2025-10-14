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
    using TheDugout.Services.GameSettings;
    using TheDugout.Services.Message;

    public class FreeAgentTransferService : IFreeAgentTransferService
    {
        private readonly DugoutDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IMessageOrchestrator _messageOrchestrator;
        private readonly IGameSettingsService _gameSettingsService;

        public FreeAgentTransferService(DugoutDbContext context, ITransactionService transactionService, IMessageOrchestrator messageOrchestrator, IGameSettingsService gameSettingsService)
        {
            _context = context;
            _transactionService = transactionService;
            _messageOrchestrator = messageOrchestrator;
            _gameSettingsService = gameSettingsService;
        }

        public async Task<(bool Success, string ErrorMessage)> SignFreePlayer(int gameSaveId, int teamId, int playerId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.GameSaveId == gameSaveId);
            var player = await _context.Players
                .Include(p => p.Agency)
                .ThenInclude(p => p.AgencyTemplate)
                .FirstOrDefaultAsync(p => p.Id == playerId && p.GameSaveId == gameSaveId);
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);

            if (team == null) return (false, "Team not found.");
            if (player == null) return (false, "Player not found.");
            if (player.TeamId != null) return (false, "Player already has a team.");
            if (bank == null) return (false, "Bank not found.");
            if (player.Agency == null) return (false, "Player has no agency.");

            var season = await _context.Seasons
                        .Include(s => s.Events)
                        .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (season == null) return (false, "No active season found.");
            bool inTransferWindow = season.Events.Any(e =>
                e.Type == SeasonEventType.TransferWindow &&
                e.Date.Date == season.CurrentDate.Date);

            if (!inTransferWindow)
                return (false, "Transfers are not allowed outside of the window.");

            // --- 💰 Разделяме парите ---
            decimal totalFee = player.Price;
            var taxSetting = await _gameSettingsService.GetDecimalAsync("SignFreePlayerBankTax");
            decimal bankTax = taxSetting ?? 0.10m;
            decimal bankCut = totalFee * bankTax;
            decimal agencyCut = totalFee - bankCut;

            // Проверка дали отборът има пари
            if (team.Balance < totalFee)
                return (false, "Team cannot afford this player.");

            // 1️⃣ Плащане към агенцията (90%)
            var txAgency = await _transactionService.ClubToAgencyAsync(
                team, player.Agency, agencyCut,
                $"Transfer fee to {player.Agency.AgencyTemplate.Name} for {player.FirstName} {player.LastName}",
                TransactionType.TransferFee);

            if (txAgency.Status != TransactionStatus.Completed)
                return (false, "Agency payment failed.");

            // 2️⃣ Плащане към банката (10%)
            var txBank = await _transactionService.ClubToBankAsync(
                team, bank, bankCut,
                $"Bank fee for {player.FirstName} {player.LastName}",
                TransactionType.BankFee);

            if (txBank.Status != TransactionStatus.Completed)
                return (false, "Bank fee payment failed.");

            // 🧾 Обновяваме агенцията
            player.Agency.TotalEarnings += agencyCut;
            _context.Agencies.Update(player.Agency);

            // 🧍‍♂️ Играчът вече има отбор, излиза от агенцията
            player.TeamId = team.Id;
            player.AgencyId = null;
            player.Agency = null;

            // 📝 Записваме трансфера
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
        public async Task<(bool Success, string ErrorMessage)> ReleasePlayerAsync(int gameSaveId, int teamId, int playerId)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.GameSaveId == gameSaveId);
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId && p.GameSaveId == gameSaveId);
            var bank = await _context.Banks.FirstOrDefaultAsync(b => b.GameSaveId == gameSaveId);

            if (team == null) return (false, "Team not found.");
            if (player == null) return (false, "Player not found.");
            if (bank == null) return (false, "Bank not found.");
            if (player.TeamId != team.Id) return (false, "Player does not belong to this team.");

            var season = await _context.Seasons
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.GameSaveId == gameSaveId && s.IsActive);

            if (season == null) return (false, "No active season found.");
            bool inTransferWindow = season.Events.Any(e =>
                e.Type == SeasonEventType.TransferWindow &&
                e.Date.Date == season.CurrentDate.Date);

            if (!inTransferWindow)
                return (false, "Transfers are not allowed outside of the window.");

            decimal totalFee = player.Price;
            var taxSetting = await _gameSettingsService.GetDecimalAsync("ReleasePlayerBankTax");
            decimal bankTax = taxSetting ?? 0.10m;
            decimal bankCut = totalFee * bankTax;
            decimal agencyCut = totalFee - bankCut;

            // 🔎 Намираме агенции, които могат да си го позволят
            var agencies = await _context.Agencies
                .Include(a => a.AgencyTemplate)
                .Where(a => a.GameSaveId == gameSaveId && a.Budget >= totalFee)
                .ToListAsync();

            if (!agencies.Any())
            {
                // Няма агенции с пари - просто освобождаваме го без плащане
                player.TeamId = null;
                player.AgencyId = null;
                await _context.SaveChangesAsync();
                return (true, "Player released but no agencies could afford him.");
            }

            // 🎯 Избираме произволна или най-богата агенция (по желание)
            var agency = agencies.OrderByDescending(a => a.Budget).First();

            // 💰 Плащане от агенцията към отбора
            var txClub = await _transactionService.AgencyToClubAsync(
                agency, team, agencyCut,
                $"Agency {agency.AgencyTemplate.Name} signs released player {player.FirstName} {player.LastName}",
                TransactionType.TransferFee);

            if (txClub.Status != TransactionStatus.Completed)
                return (false, "Payment from agency failed.");

            // 💰 Такса към банката
            var txBank = await _transactionService.AgencyToBankAsync(
                agency, bank, bankCut,
                $"Bank fee for signing {player.FirstName} {player.LastName}",
                TransactionType.BankFee);

            if (txBank.Status != TransactionStatus.Completed)
                return (false, "Bank fee transaction failed.");

            agency.TotalEarnings += agencyCut;
            _context.Agencies.Update(agency);

            // 🧾 Освобождаваме играча
            player.TeamId = null;
            player.AgencyId = agency.Id;
            _context.Players.Update(player);

            // 📜 История на трансфера
            var transfer = new Transfer
            {
                GameSaveId = gameSaveId,
                SeasonId = season.Id,
                PlayerId = player.Id,
                FromTeamId = team.Id,
                Fee = player.Price,
                IsFreeAgent = false,
                GameDate = season.CurrentDate
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();


            return (true, "");
        }

    }
}
