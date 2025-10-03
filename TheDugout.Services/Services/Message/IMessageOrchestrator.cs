using TheDugout.Models.Messages;

namespace TheDugout.Services.Message
{
    public interface IMessageOrchestrator
    {
        Task<Models.Messages.Message> SendMessageAsync(
            MessageCategory category,
            int gameSaveId,
            object contextModel
        );
    }
}
