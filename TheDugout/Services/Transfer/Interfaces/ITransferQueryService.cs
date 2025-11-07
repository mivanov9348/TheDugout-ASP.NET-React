namespace TheDugout.Services.Transfer.Interfaces
{
    public interface ITransferQueryService
    {
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

    }
}
