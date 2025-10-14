namespace TheDugout.Services.Transfer
{
    public interface IFreeAgentTransferService
    {
        Task<(bool Success, string ErrorMessage)> SignFreePlayer(int gameSaveId, int teamId, int playerId);
        Task<(bool Success, string ErrorMessage)> ReleasePlayerAsync(int gameSaveId, int teamId, int playerId);
    }
}
