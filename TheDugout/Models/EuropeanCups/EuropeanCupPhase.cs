using TheDugout.Models.Fixtures;
using TheDugout.Models.Game;

namespace TheDugout.Models.Competitions
{
    public class EuropeanCupPhase
    {
        public int Id { get; set; }

        public int EuropeanCupId { get; set; }
        public EuropeanCup EuropeanCup { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int PhaseTemplateId { get; set; }
        public EuropeanCupPhaseTemplate PhaseTemplate { get; set; } = null!;
        public bool IsQualificationPhase { get; set; } = false;

        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
    }
}
