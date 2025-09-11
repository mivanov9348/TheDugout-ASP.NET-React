    namespace TheDugout.Models
{
    public class EuropeanCupPhaseTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; 
        public int Order { get; set; }
        public bool IsKnockout { get; set; }
        public bool IsTwoLegged { get; set; }
    }
}
