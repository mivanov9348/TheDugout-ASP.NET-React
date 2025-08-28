namespace TheDugout.Models
{
    public class PlayerAttributes
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int Reflexes { get; set; }    
        public int Handling { get; set; }      
        public int Tackling { get; set; }      
        public int Positioning { get; set; }   
        public int Passing { get; set; }      
        public int Vision { get; set; }       
        public int Finishing { get; set; }     
        public int Pace { get; set; }         

        public double Average =>
            (Reflexes + Handling + Tackling + Positioning +
             Passing + Vision + Finishing + Pace) / 8.0;
    }
}
