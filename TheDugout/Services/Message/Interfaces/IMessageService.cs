namespace TheDugout.Services.Message.Interfaces
{
    using TheDugout.Models.Messages;
    public interface IMessageService
    {
        Task<Message> CreateMessageAsync(
            MessageCategory category,
            Dictionary<string, string> placeholders,
            int gameSaveId,
            bool strict = false);

        Task<Message> CreateAndSaveMessageAsync(
            MessageCategory category,
            Dictionary<string, string> placeholders,
            int gameSaveId,
            bool strict = false);
    }
}
