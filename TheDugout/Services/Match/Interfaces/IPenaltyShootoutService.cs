namespace TheDugout.Services.Match.Interfaces
{
    public interface IPenaltyShootoutService
    {
        Task<Models.Matches.Match> RunPenaltyShootoutAsync(Models.Matches.Match match);

    }
}
