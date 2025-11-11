namespace TheDugout.Services.Transfer
{
    public interface ICPUTransferService
    {
        Task RunCpuTransfersAsync(int gameSaveId, int seasonId, DateTime date, int teamId);

    }
}
