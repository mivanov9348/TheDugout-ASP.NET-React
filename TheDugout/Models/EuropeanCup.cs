namespace TheDugout.Models
{
    public class EuropeanCup
    {
        public int Id { get; set; }

        public int TemplateId { get; set; }
        public EuropeanCupTemplate Template { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public ICollection<EuropeanCupTeam> Teams { get; set; } = new List<EuropeanCupTeam>();
        public ICollection<EuropeanCupPhase> Phases { get; set; } = new List<EuropeanCupPhase>();
        public ICollection<EuropeanCupStanding> Standings { get; set; } = new List<EuropeanCupStanding>();
    }

}

