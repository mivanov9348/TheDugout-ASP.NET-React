using TheDugout.Models;

namespace TheDugout.Services.Finance
{
    public interface IFinanceService
    {
        Task<Bank> CreateBankAsync(GameSave gameSave, decimal initialCapital = 1000000);
    }
}
