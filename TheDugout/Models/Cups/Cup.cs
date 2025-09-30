using TheDugout.Models.Common;
using TheDugout.Models.Game;
using TheDugout.Models.Seasons;

namespace TheDugout.Models.Cups
{
    public class Cup
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public CupTemplate Template { get; set; } = null!;
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;
        public int TeamsCount { get; set; }     
        public int RoundsCount { get; set; }    
        public bool IsActive { get; set; } = true;
        public string? LogoFileName { get; set; }
        public ICollection<CupTeam> Teams { get; set; } = new List<CupTeam>();
        public ICollection<CupRound> Rounds { get; set; } = new List<CupRound>();
    }
}
