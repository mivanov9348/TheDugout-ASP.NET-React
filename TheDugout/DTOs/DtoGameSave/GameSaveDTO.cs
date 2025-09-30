namespace TheDugout.DTOs.DtoGameSave
{
    public class GameSaveDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int? UserTeamId { get; set; }
        public string? UserTeamName { get; set; }

        public TeamHeaderDto? UserTeam { get; set; }

        public IEnumerable<LeagueDto> Leagues { get; set; } = new List<LeagueDto>();
        public IEnumerable<SeasonDto> Seasons { get; set; } = new List<SeasonDto>();

        public string? NextDayActionKey { get; set; }
        public string NextDayActionLabel { get; set; } = "Next Day →";

    }

    public class TeamHeaderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public decimal Balance { get; set; }

        public int? LeagueId { get; set; }
        public string? LeagueName { get; set; }

        public int? CountryId { get; set; }
        public string CountryName { get; set; } = null!;
    }

    public class LeagueDto
    {
        public int Id { get; set; }
        public int Tier { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; } = null!;
        public string LeagueName { get; set; } = null!;
        public int TeamsCount { get; set; }

        public IEnumerable<TeamDto> Teams { get; set; } = new List<TeamDto>();
    }

    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public int? CountryId { get; set; }
        public string CountryName { get; set; } = null!;
    }

    public class SeasonDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CurrentDate { get; set; }

        public IEnumerable<EventDto> Events { get; set; } = new List<EventDto>();
    }

    public class EventDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = null!; // CupMatch, ChampionshipMatch, EuropeanMatch
        public string Description { get; set; } = null!;
    }
}
