using TheDugout.Data.DtoGameSave;
using TheDugout.Models;

public static class GameSaveExtensions
{
    public static GameSaveDto ToDto(this GameSave gs)
    {
        return new GameSaveDto
        {
            Id = gs.Id,
            Name = gs.Name,
            CreatedAt = gs.CreatedAt,

            UserTeamId = gs.UserTeamId,
            UserTeamName = gs.UserTeam?.Name,

            Leagues = gs.Leagues?.Select(l => new LeagueDto
            {
                Id = l.Id,
                Tier = l.Tier,
                CountryId = l.CountryId,
                CountryName = l.Country?.Name ?? "Unknown", // Проверка за null
                LeagueName = l.Template?.Name ?? "Unknown", // Проверка за null
                TeamsCount = l.TeamsCount,
                Teams = l.Teams?.Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Abbreviation = t.Abbreviation ?? "N/A", // Проверка за null
                    CountryId = t.CountryId,
                    CountryName = t.Country?.Name ?? "Unknown" // Проверка за null
                })?.ToList() ?? new List<TeamDto>()
            })?.ToList() ?? new List<LeagueDto>(),

            Seasons = gs.Seasons?.Select(s => new SeasonDto
            {
                Id = s.Id,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })?.ToList() ?? new List<SeasonDto>()
        };
    }
}