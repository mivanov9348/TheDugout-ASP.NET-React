// Models/EuropeanCupTemplate.cs
namespace TheDugout.Models
{
    public class EuropeanCupTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Champions League";
        public int TeamsCount { get; set; } = 36;
        public int LeaguePhaseMatchesPerTeam { get; set; } = 8;
        public int Ranking { get; set; }    
        public bool IsActive { get; set; }
        public ICollection<EuropeanCupPhaseTemplate> PhaseTemplates { get; set; } = new List<EuropeanCupPhaseTemplate>();
    }

   
}
