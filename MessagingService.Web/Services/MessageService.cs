using MessagingService.Web.Models;
using MessagingService.Web.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MessagingService.Web.Services;

public class MessageService(IMessageRepository messageRepository, ILogger<MessageService> logger)
{
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly ILogger<MessageService> _logger = logger;

    public async Task AddMessageAsync(Message message)
    {
        message.Timestamp = DateTime.UtcNow;
        try
        {
            await _messageRepository.AddMessageAsync(message);
            _logger.LogInformation("Message added: {Text}", message.Text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message: {ErrorMessage}", ex.Message);
            throw; // Re-throw or handle
        }
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(DateTime startTime, DateTime endTime)
    {
        return await _messageRepository.GetMessagesAsync(startTime, endTime);
    }
}