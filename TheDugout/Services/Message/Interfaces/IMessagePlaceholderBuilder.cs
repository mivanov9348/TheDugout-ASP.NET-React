namespace TheDugout.Services.Message.Interfaces
{
    using TheDugout.Models.Messages;
    public interface IMessagePlaceholderBuilder
    {
        MessageCategory Category { get; }
        Dictionary<string, string> Build(object contextModel);
    }
}
