namespace TheDugout.Services.Message
{
    using TheDugout.Models.Messages;
    using TheDugout.Models.Players;
    using TheDugout.Services.Message.Interfaces;

    public class RetirementMessageBuilder : IMessagePlaceholderBuilder
    {
        public MessageCategory Category => MessageCategory.Retirement;

        public Dictionary<string, string> Build(object contextModel)
        {
            var player = (Player)contextModel;

            var playerName = $"{player.FirstName} {player.LastName}";
            var age = player.GetAge(player.GameSave.Seasons.FirstOrDefault(s => s.IsActive)?.CurrentDate ?? DateTime.UtcNow);
            var teamName = player.Team?.Name ?? "former club";

            return new()
            {
                ["PlayerName"] = playerName,
                ["Age"] = age.ToString(),
                ["TeamName"] = teamName
            };
        }
    }
}
