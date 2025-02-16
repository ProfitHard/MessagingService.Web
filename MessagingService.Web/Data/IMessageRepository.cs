using MessagingService.Web.Models;

namespace MessagingService.Web.Data;

public interface IMessageRepository
{
    IAsyncEnumerable<Message> GetMessagesAsync(DateTime startTime, DateTime endTime);
    Task AddMessageAsync(Message message);
}