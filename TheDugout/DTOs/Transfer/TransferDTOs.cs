namespace TheDugout.DTOs.Transfer
{
    public class BuyPlayerRequest
    {
        public int GameSaveId { get; set; }
        public int TeamId { get; set; }   
        public int PlayerId { get; set; }
    }
}
