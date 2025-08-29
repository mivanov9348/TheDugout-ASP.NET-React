namespace TheDugout.Models
{
    public class PlayerAttribute
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public string Name { get; set; } = null!;
        public int Value { get; set; } 
    }
}
