using MessagingService.Web.Data;
using MessagingService.Web.Models;

namespace MessagingService.Web.Services;

public class MessageService
{
    private readonly IMessageRepository _messageRepository;

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public IAsyncEnumerable<Message> GetMessagesAsync(DateTime startTime, DateTime endTime)
    {
        return _messageRepository.GetMessagesAsync(startTime, endTime);
    }

    public async Task AddMessageAsync(Message message)
    {
        await _messageRepository.AddMessageAsync(message);
    }
}