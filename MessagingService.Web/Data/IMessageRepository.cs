using MessagingService.Web.Models;

namespace MessagingService.Web.Data;

public interface IMessageRepository
{
    Task<int> AddMessageAsync(Message message);
    Task<IEnumerable<Message>> GetMessagesAsync(DateTime startTime, DateTime endTime);
}