    namespace TheDugout.Services.Message
{
    using TheDugout.Models.Messages;
    using TheDugout.Services.Message.Interfaces;

    public class MessageOrchestrator : IMessageOrchestrator
    {
        private readonly IMessageService _messageService;
        private readonly IEnumerable<IMessagePlaceholderBuilder> _builders;
        private readonly ILogger<MessageOrchestrator> _logger;

        public MessageOrchestrator(
            IMessageService messageService,
            IEnumerable<IMessagePlaceholderBuilder> builders,
            ILogger<MessageOrchestrator> logger)
        {
            _messageService = messageService;
            _builders = builders;
            _logger = logger;
        }
        public async Task<Message> SendMessageAsync(MessageCategory category, int gameSaveId, object contextModel)
        {
            var builder = _builders.FirstOrDefault(b => b.Category == category);
            if (builder == null)
            {
                _logger.LogWarning("No placeholder builder found for category {Category}", category);
                return await _messageService.CreateAndSaveMessageAsync(category, new(), gameSaveId);
            }

            var placeholders = builder.Build(contextModel);
            return await _messageService.CreateAndSaveMessageAsync(category, placeholders, gameSaveId);
        }
    }
}