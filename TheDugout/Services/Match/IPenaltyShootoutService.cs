namespace TheDugout.Services.Match
{
    public interface IPenaltyShootoutService
    {
        Task<Models.Matches.Match> RunPenaltyShootoutAsync(Models.Matches.Match match);

    }
}
