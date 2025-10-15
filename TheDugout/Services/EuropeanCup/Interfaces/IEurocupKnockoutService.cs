namespace TheDugout.Services.EuropeanCup.Interfaces
{
    public interface IEurocupKnockoutService
    {        
        Task DeterminePostGroupAdvancementAsync(int europeanCupId);       
        Task GeneratePlayoffRoundAsync(int europeanCupId);
        Task<object> GetKnockoutFixturesAsync(int europeanCupId);
        Task GenerateNextKnockoutPhaseAsync(int europeanCupId, int currentOrder);
    }
}
