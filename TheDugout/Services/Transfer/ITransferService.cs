using TheDugout.DTOs.Transfer;

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
    int pageSize,
    int? minAge = null,
    int? maxAge = null,
    decimal? minPrice = null,
    decimal? maxPrice = null);

        Task<IEnumerable<object>> GetTransferHistoryAsync(int gameSaveId, bool onlyMine);

        Task RunCpuTransfersAsync(int gameSaveId, int seasonId, DateTime date, int teamId);

        Task<(bool Success, string ErrorMessage)> SendOfferAsync(TransferOfferRequest request);
            }
}
