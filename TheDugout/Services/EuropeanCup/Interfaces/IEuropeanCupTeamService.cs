namespace TheDugout.Services.EuropeanCup.Interfaces
{
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Teams;
    public interface IEuroCupTeamService
    {
        Task<List<EuropeanCupTeam>> CreateTeamsForCupAsync(EuropeanCup euroCup, List<Team> chosenTeams, CancellationToken ct = default);
        Task MarkTeamEliminatedAsync(int europeanCupId, int teamId, CancellationToken ct = default);
        Task MarkTeamsEliminatedAsync(int europeanCupId, List<int> teamIds, CancellationToken ct = default);
        Task<List<EuropeanCupTeam>> GetActiveTeamsAsync(int europeanCupId, CancellationToken ct = default);
    }

}
