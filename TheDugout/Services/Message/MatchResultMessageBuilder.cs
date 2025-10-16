namespace TheDugout.Services.Message
{
    using TheDugout.Models.Messages;
    using TheDugout.Services.Message.Interfaces;
    using TheDugout.Models.Matches;

    public class MatchResultMessageBuilder : IMessagePlaceholderBuilder
    {
        public MessageCategory Category => MessageCategory.MatchResult;

        public Dictionary<string, string> Build(object contextModel)
        {
            var match = (Match)contextModel;
            return new()
            {
                ["ClubName"] = match.Fixture.HomeTeam.Name,
                ["OpponentName"] = match.Fixture.AwayTeam.Name,
                ["Score"] = $"{match.Fixture.HomeTeamGoals}-{match.Fixture.HomeTeamGoals}"
            };
        }
    }
}
