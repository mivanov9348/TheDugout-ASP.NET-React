namespace TheDugout.DTOs.Transfer
{
    public class BuyPlayerRequest
    {
        public int GameSaveId { get; set; }
        public int? TeamId { get; set; }   
        public int PlayerId { get; set; }
    }

    public class TransferOfferRequest
    {
        public int GameSaveId { get; set; }
        public int FromTeamId { get; set; }
        public int ToTeamId { get; set; }
        public int PlayerId { get; set; }
        public decimal OfferAmount { get; set; }
    }

    public class ReleasePlayerRequest
    {
        public int GameSaveId { get; set; }
        public int PlayerId { get; set; }
    }
}
