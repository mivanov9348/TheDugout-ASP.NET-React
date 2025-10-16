namespace TheDugout.Services.Message
{
    using TheDugout.Models.Messages;
    using TheDugout.Services.Message.Interfaces;

    public class TransferMessageBuilder : IMessagePlaceholderBuilder
    {
        public MessageCategory Category => MessageCategory.Transfer;

        public Dictionary<string, string> Build(object contextModel)
        {
            var transfer = (Models.Transfers.Transfer)contextModel;

            var playerName = transfer.Player != null
                ? $"{transfer.Player.FirstName} {transfer.Player.LastName}"
                : "Unknown Player";

            var clubName = transfer.ToTeam?.Name ?? "Unknown Club";

            return new()
            {
                ["PlayerName"] = playerName,
                ["ClubName"] = clubName,
                ["Amount"] = transfer.Fee.ToString("N0")
            };
        }
    }
}
