using MessagingService.Web.Data;
using MessagingService.Web.Models;
using Microsoft.Extensions.Logging;

namespace MessagingService.Web.Services;

public class MessageService(IMessageRepository messageRepository, ILogger<MessageService> logger)
{
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly ILogger<MessageService> _logger = logger;

    public IAsyncEnumerable<Message> GetMessagesAsync(DateTime startTime, DateTime endTime)
    {
        _logger.LogInformation("Getting messages between {startTime} and {endTime}", startTime, endTime);
        return _messageRepository.GetMessagesAsync(startTime, endTime);
    }

    public async Task AddMessageAsync(Message message)
    {
        _logger.LogInformation("Adding message with Id: {message.Id} and Text: {message.Text}", message.Id, message.Text);
        await _messageRepository.AddMessageAsync(message);
    }
}