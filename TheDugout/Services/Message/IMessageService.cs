using TheDugout.Models.Messages;

namespace TheDugout.Services.Message
{
    public interface IMessageService
    {
        Task<Models.Messages.Message> CreateMessageAsync(
MessageCategory category,
Dictionary<string, string> placeholders,
int? gameSaveId = null,
bool strict = false);


        Task<Models.Messages.Message> CreateAndSaveMessageAsync(
        MessageCategory category,
        Dictionary<string, string> placeholders,
        int? gameSaveId = null,
        bool strict = false);
    }
}
