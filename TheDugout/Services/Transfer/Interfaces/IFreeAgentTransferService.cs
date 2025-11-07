namespace TheDugout.Services.Transfer.Interfaces
{
    public interface IFreeAgentTransferService
    {
        Task<(bool Success, string ErrorMessage)> SignFreePlayer(int gameSaveId, int teamId, int playerId);
        Task<(bool Success, string ErrorMessage)> ReleasePlayerAsync(int gameSaveId, int teamId, int playerId);
    }
}
