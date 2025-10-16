namespace TheDugout.Services.Message.Interfaces
{
    using TheDugout.Models.Messages;
    public interface IMessageOrchestrator
    {
        Task<Message> SendMessageAsync(MessageCategory category, int gameSaveId, object contextModel);
    }
}
