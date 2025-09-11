namespace TheDugout.Models
{
    public class EuropeanCupTeam
    {
        public int Id { get; set; }

        public int EuropeanCupId { get; set; }
        public EuropeanCup EuropeanCup { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;


    }
}
