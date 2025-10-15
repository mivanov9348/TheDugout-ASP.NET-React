namespace TheDugout.Models.Cups
{
    using TheDugout.Models.Common;
    using TheDugout.Models.Competitions;
    using TheDugout.Models.Game;
    using TheDugout.Models.Seasons;
    public class Cup
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public CupTemplate Template { get; set; } = null!;
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int? SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public int? CountryId { get; set; }
        public Country Country { get; set; } = null!;
        public int? CompetitionId { get; set; }
        public Competition? Competition { get; set; } = null!;
        public int TeamsCount { get; set; }     
        public int RoundsCount { get; set; }    
        public bool IsActive { get; set; } = true;
        public bool IsFinished { get; set; } = false;
        public string? LogoFileName { get; set; }
        public ICollection<CupTeam> Teams { get; set; } = new List<CupTeam>();
        public ICollection<CupRound> Rounds { get; set; } = new List<CupRound>();

    }
}
