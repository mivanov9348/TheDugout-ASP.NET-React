using TheDugout.Data.DtoGameSave;
using TheDugout.Data.DtoNewGame;
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
            Leagues = gs.Leagues.Select(l => new LeagueDto
            {
                Id = l.Id,
                Tier = l.Tier,
                CountryId = l.CountryId,
                TeamsCount = l.TeamsCount,
                Teams = l.Teams.Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Abbreviation = t.Abbreviation,
                    CountryId = t.CountryId
                })
            }),
            Seasons = gs.Seasons.Select(s => new SeasonDto
            {
                Id = s.Id,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })
        };
    }
}
