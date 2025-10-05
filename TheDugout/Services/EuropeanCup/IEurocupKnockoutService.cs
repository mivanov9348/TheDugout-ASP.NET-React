namespace TheDugout.Services.EuropeanCup
{
    public interface IEurocupKnockoutService
    {        
        Task DeterminePostGroupAdvancementAsync(int europeanCupId);       
        Task GeneratePlayoffRoundAsync(int europeanCupId);
    }
}
