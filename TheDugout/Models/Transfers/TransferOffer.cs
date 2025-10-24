namespace TheDugout.Models.Transfers
{
    using TheDugout.Models.Game;
    using TheDugout.Models.Players;
    using TheDugout.Models.Seasons;
    using TheDugout.Models.Teams;
    public enum OfferStatus
    {
        Pending,
        Accepted,
        Rejected
    }
    public class TransferOffer
    {
        public int Id { get; set; }
        public int GameSaveId { get; set; }
        public GameSave GameSave { get; set; } = null!;
        public int? SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public int FromTeamId { get; set; }
        public Team FromTeam { get; set; } = null!;
        public int ToTeamId { get; set; }
        public Team ToTeam { get; set; } = null!;
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public decimal OfferAmount { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        public DateTime CreatedAt { get; set; }
    }
}
