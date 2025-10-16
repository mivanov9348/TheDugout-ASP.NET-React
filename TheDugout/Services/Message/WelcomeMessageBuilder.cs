namespace TheDugout.Services.Message
{
    using TheDugout.Models.Messages;
    using TheDugout.Services.Message.Interfaces;
    public class WelcomeMessageBuilder : IMessagePlaceholderBuilder
    {
        public MessageCategory Category => MessageCategory.Welcome;

        public Dictionary<string, string> Build(object contextModel)
        {
            var (user, team) = ((Models.Game.User user, Models.Teams.Team team))contextModel;
            return new()
            {
                ["ManagerName"] = user.Username,
                ["ClubName"] = team.Name
            };
        }
    }

}
