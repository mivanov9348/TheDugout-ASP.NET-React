namespace TheDugout.Models.Competitions
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Teams;
    public class EuropeanCupTeam
    {
        public int Id { get; set; }

        public int EuropeanCupId { get; set; }
        public EuropeanCup EuropeanCup { get; set; } = null!;

        public int? TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;

        public int CurrentPhaseOrder { get; set; } = 0;
        public bool IsEliminated { get; set; } = false;
        public bool IsPlayoffParticipant { get; set; } = false;


    }
}
