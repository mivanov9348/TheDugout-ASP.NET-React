namespace TheDugout.Models
{
    public class EuropeanCupPhase
    {
        public int Id { get; set; }

        public int EuropeanCupId { get; set; }
        public EuropeanCup EuropeanCup { get; set; } = null!;

        public int PhaseTemplateId { get; set; }
        public EuropeanCupPhaseTemplate PhaseTemplate { get; set; } = null!;

        public ICollection<EuropeanCupMatch> Matches { get; set; } = new List<EuropeanCupMatch>();
    }
}
