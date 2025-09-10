namespace TheDugout.Services.Transfer
{
    public interface ITransferService
    {
        Task<(bool Success, string ErrorMessage)> BuyPlayerAsync(int gameSaveId, int teamId, int playerId);

        Task<object> GetPlayersAsync(
            int gameSaveId,
            string? search,
            string? team,
            string? country,
            string? position,
            bool freeAgent,
            string sortBy,
            string sortOrder,
            int page,
            int pageSize);

        Task<IEnumerable<object>> GetTransferHistoryAsync(int gameSaveId, bool onlyMine);
    }
}
