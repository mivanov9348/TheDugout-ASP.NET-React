using TheDugout.Models.Cups;

namespace TheDugout.Services.Cup
{
    public interface ICupFixturesService
    {
        Task GenerateInitialFixturesAsync(int seasonId, int gameSaveId, List<Models.Cups.Cup> cups);
        Task GenerateNextRoundAsync(int cupId, int gameSaveId, int seasonId);
        bool IsRoundFinished(CupRound round);
    }
}
