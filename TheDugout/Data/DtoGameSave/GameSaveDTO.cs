namespace TheDugout.Data.DtoGameSave
{
    public class GameSaveDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public IEnumerable<LeagueDto> Leagues { get; set; } = new List<LeagueDto>();
        public IEnumerable<SeasonDto> Seasons { get; set; } = new List<SeasonDto>();
    }

    public class LeagueDto
    {
        public int Id { get; set; }
        public int Tier { get; set; }
        public int CountryId { get; set; }
        public int TeamsCount { get; set; }

        public IEnumerable<TeamDto> Teams { get; set; } = new List<TeamDto>();
    }

    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public int CountryId { get; set; }
    }

    public class SeasonDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


}
