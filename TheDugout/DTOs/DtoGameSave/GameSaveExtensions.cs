namespace TheDugout.DTOs.DtoGameSave
{

    using TheDugout.DTOs.DtoGameSave;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;

    public static class GameSaveExtensions
    {
        public static GameSaveDto ToDto(this GameSave gs)
        {
            var dto = new GameSaveDto
            {
                Id = gs.Id,
                Name = gs.Name,
                CreatedAt = gs.CreatedAt,

                UserTeamId = gs.UserTeamId,
                UserTeamName = gs.UserTeam?.Name,

                UserTeam = gs.UserTeam == null ? null : new TeamHeaderDto
                {
                    Id = gs.UserTeam.Id,
                    Name = gs.UserTeam.Name,
                    Abbreviation = gs.UserTeam.Abbreviation ?? "N/A",
                    Balance = gs.UserTeam.Balance,
                    LeagueId = gs.UserTeam.LeagueId,
                    LeagueName = gs.UserTeam.League?.Template?.Name
                                 ?? gs.UserTeam.League?.Country?.Name
                                 ?? "Unknown",
                    CountryId = gs.UserTeam.CountryId,
                    CountryName = gs.UserTeam.Country?.Name ?? "Unknown"
                },

                Leagues = gs.Leagues?.Select(l => new LeagueDto
                {
                    Id = l.Id,
                    Tier = l.Tier,
                    CountryId = l.CountryId,
                    CountryName = l.Country?.Name ?? "Unknown",
                    LeagueName = l.Template?.Name ?? "Unknown",
                    TeamsCount = l.TeamsCount,
                    Teams = l.Teams?.Select(t => new TeamDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Abbreviation = t.Abbreviation ?? "N/A",
                        CountryId = t.CountryId,
                        CountryName = t.Country?.Name ?? "Unknown"
                    }).ToList() ?? new List<TeamDto>()
                }).ToList() ?? new List<LeagueDto>(),

                Seasons = gs.Seasons?.Select(s => new SeasonDto
                {
                    Id = s.Id,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    CurrentDate = s.CurrentDate,
                    IsActive = s.IsActive  
                }).ToList() ?? new List<SeasonDto>()
            };

            // 🔥 Ето това е ключовото:
            dto.ActiveSeason = gs.Seasons?
                .FirstOrDefault(s => s.IsActive)
                ?.ToDto()
                ?? gs.Seasons?.OrderByDescending(s => s.Id).FirstOrDefault()?.ToDto();

            return dto;
        }

        private static SeasonDto ToDto(this Season s)
        {
            return new SeasonDto
            {
                Id = s.Id,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                CurrentDate = s.CurrentDate,
                IsActive = s.IsActive
            };
        }
    }
}